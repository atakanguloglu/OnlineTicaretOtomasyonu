using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Data;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;
using System.Text.RegularExpressions;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IMapper _mapper;

        public OrdersController(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IMapper mapper)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string search = null,
            [FromQuery] string status = null,
            [FromQuery] string paymentStatus = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var query = _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.TenantId == tenantId.Value);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.OrderNumber.Contains(search) ||
                                         o.Customer.FirstName.Contains(search) ||
                                         o.Customer.LastName.Contains(search) ||
                                         o.Customer.Email.Contains(search) ||
                                         o.ShippingName.Contains(search));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Apply payment status filter
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(o => o.PaymentStatus == paymentStatus);
            }

            // Apply date range filter
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            // Order by most recent orders first
            query = query.OrderByDescending(o => o.OrderDate);

            // Count before pagination for total count
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            var orderViewModels = _mapper.Map<IEnumerable<OrderViewModel>>(orders);
            
            // Create paged result
            var pagedResult = PagedResultViewModel<OrderViewModel>.Create(
                orderViewModels, 
                totalCount, 
                pageNumber, 
                pageSize);
                
            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<OrderViewModel>(order));
        }

        [HttpPost]
        [Authorize(Roles = "TenantAdmin,TenantManager,TenantStaff")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            // Validate customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == model.CustomerId && c.TenantId == tenantId.Value);

            if (customer == null)
            {
                return BadRequest("Geçersiz müşteri");
            }

            // Generate order number
            string orderNumber = GenerateOrderNumber();

            // Validate product items
            if (model.OrderItems == null || !model.OrderItems.Any())
            {
                return BadRequest("Sipariş kalemleri boş olamaz");
            }

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                OrderDate = model.OrderDate,
                Status = model.Status,
                PaymentStatus = model.PaymentStatus,
                PaymentMethod = model.PaymentMethod,
                TotalAmount = model.TotalAmount,
                TaxAmount = model.TaxAmount,
                ShippingAmount = model.ShippingAmount,
                DiscountAmount = model.DiscountAmount,
                Notes = model.Notes,
                ShippingName = model.ShippingName,
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingCountry = model.ShippingCountry,
                ShippingPostalCode = model.ShippingPostalCode,
                ShippingPhone = model.ShippingPhone,
                CustomerId = model.CustomerId,
                TenantId = tenantId.Value
            };

            // Add order to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add order items
            foreach (var itemModel in model.OrderItems)
            {
                // Validate product
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == itemModel.ProductId && p.TenantId == tenantId.Value);

                if (product == null)
                {
                    return BadRequest($"Ürün bulunamadı: {itemModel.ProductId}");
                }

                // Check stock
                if (product.StockQuantity < itemModel.Quantity)
                {
                    return BadRequest($"Yetersiz stok: {product.Name}, Mevcut: {product.StockQuantity}, İstenen: {itemModel.Quantity}");
                }

                // Add order item
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemModel.ProductId,
                    ProductName = product.Name,
                    UnitPrice = itemModel.UnitPrice,
                    Quantity = itemModel.Quantity,
                    TotalPrice = itemModel.TotalPrice,
                    TaxRate = itemModel.TaxRate,
                    TaxAmount = itemModel.TaxAmount,
                    DiscountAmount = itemModel.DiscountAmount
                };

                _context.OrderItems.Add(orderItem);

                // Update product stock
                product.StockQuantity -= itemModel.Quantity;
                _context.Products.Update(product);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, _mapper.Map<OrderViewModel>(order));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "TenantAdmin,TenantManager,TenantStaff")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateViewModel model)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId.Value);

            if (order == null)
            {
                return NotFound();
            }

            // Update order status
            order.Status = model.Status;
            if (!string.IsNullOrEmpty(model.PaymentStatus))
            {
                order.PaymentStatus = model.PaymentStatus;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Sipariş durumu güncellendi", status = order.Status, paymentStatus = order.PaymentStatus });
        }

        [HttpGet("statuses")]
        public IActionResult GetOrderStatuses()
        {
            var orderStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            return Ok(orderStatuses);
        }

        [HttpGet("payment-statuses")]
        public IActionResult GetPaymentStatuses()
        {
            var paymentStatuses = new[] { "Pending", "Paid", "Failed", "Refunded" };
            return Ok(paymentStatuses);
        }

        [HttpGet("payment-methods")]
        public IActionResult GetPaymentMethods()
        {
            var paymentMethods = new[] { "Cash", "Credit Card", "Bank Transfer", "Online Payment" };
            return Ok(paymentMethods);
        }

        private string GenerateOrderNumber()
        {
            // Generate order number in format: ORD-YYYYMMDD-XXXX where XXXX is a random number
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            Random random = new Random();
            int randomNum = random.Next(1000, 10000);
            return $"ORD-{dateStr}-{randomNum}";
        }
    }

    public class OrderStatusUpdateViewModel
    {
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
    }
} 