using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Data;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels.Reports;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "TenantAdmin,TenantManager")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public ReportsController(
            ApplicationDbContext context,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // Total sales today
            var salesTodayTask = _context.Orders
                .Where(o => o.TenantId == tenantId.Value && o.OrderDate.Date == today && o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);

            // Total sales this month
            var salesThisMonthTask = _context.Orders
                .Where(o => o.TenantId == tenantId.Value && o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth && o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);

            // Total sales last month
            var salesLastMonthTask = _context.Orders
                .Where(o => o.TenantId == tenantId.Value && o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth && o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);

            // New orders today
            var newOrdersTodayTask = _context.Orders
                .CountAsync(o => o.TenantId == tenantId.Value && o.OrderDate.Date == today);

            // Pending orders
            var pendingOrdersTask = _context.Orders
                .CountAsync(o => o.TenantId == tenantId.Value && o.Status == "Pending");

            // Processing orders
            var processingOrdersTask = _context.Orders
                .CountAsync(o => o.TenantId == tenantId.Value && o.Status == "Processing");

            // Total customers
            var totalCustomersTask = _context.Customers
                .CountAsync(c => c.TenantId == tenantId.Value && c.IsActive);

            // Low stock products
            var lowStockProductsTask = _context.Products
                .CountAsync(p => p.TenantId == tenantId.Value && p.IsActive && p.StockQuantity <= 10);

            // Top selling products
            var topSellingProductsTask = _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Product.TenantId == tenantId.Value)
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new TopSellingProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalAmount = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(5)
                .ToListAsync();

            // Sales by category
            var salesByCategoryTask = _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Product.TenantId == tenantId.Value)
                .GroupBy(oi => new { oi.Product.Category.Id, oi.Product.Category.Name })
                .Select(g => new SalesByCategoryViewModel
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    TotalAmount = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(5)
                .ToListAsync();

            // Recent orders
            var recentOrdersTask = _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.TenantId == tenantId.Value)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                })
                .ToListAsync();

            // Wait for all tasks to complete
            await Task.WhenAll(
                salesTodayTask, salesThisMonthTask, salesLastMonthTask,
                newOrdersTodayTask, pendingOrdersTask, processingOrdersTask,
                totalCustomersTask, lowStockProductsTask,
                topSellingProductsTask, salesByCategoryTask, recentOrdersTask
            );

            // Calculate month-over-month change
            decimal monthOverMonthChange = 0;
            if (await salesLastMonthTask > 0)
            {
                monthOverMonthChange = ((await salesThisMonthTask - await salesLastMonthTask) / await salesLastMonthTask) * 100;
            }

            var dashboardData = new DashboardViewModel
            {
                SalesToday = await salesTodayTask,
                SalesThisMonth = await salesThisMonthTask,
                SalesLastMonth = await salesLastMonthTask,
                MonthOverMonthChange = monthOverMonthChange,
                NewOrdersToday = await newOrdersTodayTask,
                PendingOrders = await pendingOrdersTask,
                ProcessingOrders = await processingOrdersTask,
                TotalCustomers = await totalCustomersTask,
                LowStockProducts = await lowStockProductsTask,
                TopSellingProducts = await topSellingProductsTask,
                SalesByCategory = await salesByCategoryTask,
                RecentOrders = await recentOrdersTask
            };

            return Ok(dashboardData);
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string groupBy = "day")
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            if (!fromDate.HasValue)
            {
                fromDate = DateTime.UtcNow.AddDays(-30).Date;
            }

            if (!toDate.HasValue)
            {
                toDate = DateTime.UtcNow.Date;
            }

            var query = _context.Orders
                .Where(o => o.TenantId == tenantId.Value && o.Status != "Cancelled" && o.OrderDate >= fromDate && o.OrderDate <= toDate.Value.AddDays(1).AddSeconds(-1));

            var result = new List<SalesReportItemViewModel>();

            switch (groupBy.ToLower())
            {
                case "month":
                    result = await query
                        .GroupBy(o => new { Year = o.OrderDate.Year, Month = o.OrderDate.Month })
                        .Select(g => new SalesReportItemViewModel
                        {
                            Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                            TotalAmount = g.Sum(o => o.TotalAmount),
                            TaxAmount = g.Sum(o => o.TaxAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Date)
                        .ToListAsync();
                    break;

                case "week":
                    result = await query
                        .GroupBy(o => new { Year = o.OrderDate.Year, Week = ((o.OrderDate.DayOfYear - 1) / 7) + 1 })
                        .Select(g => new SalesReportItemViewModel
                        {
                            Date = new DateTime(g.Key.Year, 1, 1).AddDays((g.Key.Week - 1) * 7),
                            TotalAmount = g.Sum(o => o.TotalAmount),
                            TaxAmount = g.Sum(o => o.TaxAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Date)
                        .ToListAsync();
                    break;

                default: // day
                    result = await query
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new SalesReportItemViewModel
                        {
                            Date = g.Key,
                            TotalAmount = g.Sum(o => o.TotalAmount),
                            TaxAmount = g.Sum(o => o.TaxAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Date)
                        .ToListAsync();
                    break;
            }

            return Ok(new SalesReportViewModel
            {
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                GroupBy = groupBy,
                TotalSales = result.Sum(r => r.TotalAmount),
                TotalOrders = result.Sum(r => r.OrderCount),
                Items = result
            });
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryReport([FromQuery] int? categoryId = null, [FromQuery] int minStock = 0, [FromQuery] int maxStock = int.MaxValue)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId.Value && p.IsActive && p.StockQuantity >= minStock && p.StockQuantity <= maxStock);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .Select(p => new InventoryReportItemViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category.Name,
                    SKU = p.SKU,
                    StockQuantity = p.StockQuantity,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.Price,
                    StockValue = p.StockQuantity * p.CostPrice
                })
                .OrderBy(p => p.CategoryName)
                .ThenBy(p => p.ProductName)
                .ToListAsync();

            return Ok(new InventoryReportViewModel
            {
                TotalProducts = products.Count,
                TotalStockValue = products.Sum(p => p.StockValue),
                Items = products
            });
        }
    }
} 