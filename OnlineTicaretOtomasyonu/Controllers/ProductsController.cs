using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Data;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IMapper _mapper;

        public ProductsController(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IMapper mapper)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string search = null, [FromQuery] int? categoryId = null, [FromQuery] bool? active = null)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId.Value);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || 
                                         p.Description.Contains(search) || 
                                         p.SKU.Contains(search) || 
                                         p.Barcode.Contains(search));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply active filter
            if (active.HasValue)
            {
                query = query.Where(p => p.IsActive == active.Value);
            }

            var products = await query.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<ProductViewModel>>(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId.Value);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductViewModel>(product));
        }

        [HttpPost]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductViewModel model)
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

            // Validate category
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == model.CategoryId && c.TenantId == tenantId.Value);

            if (category == null)
            {
                return BadRequest("Geçersiz kategori");
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                DiscountPrice = model.DiscountPrice,
                StockQuantity = model.StockQuantity,
                SKU = model.SKU,
                Barcode = model.Barcode,
                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                TenantId = tenantId.Value,
                TaxRate = model.TaxRate,
                CostPrice = model.CostPrice,
                Weight = model.Weight,
                Length = model.Length,
                Width = model.Width,
                Height = model.Height,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, _mapper.Map<ProductViewModel>(product));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId.Value);

            if (product == null)
            {
                return NotFound();
            }

            // Validate category
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == model.CategoryId && c.TenantId == tenantId.Value);

            if (category == null)
            {
                return BadRequest("Geçersiz kategori");
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.DiscountPrice = model.DiscountPrice;
            product.StockQuantity = model.StockQuantity;
            product.SKU = model.SKU;
            product.Barcode = model.Barcode;
            product.IsActive = model.IsActive;
            product.IsFeatured = model.IsFeatured;
            product.ImageUrl = model.ImageUrl;
            product.CategoryId = model.CategoryId;
            product.TaxRate = model.TaxRate;
            product.CostPrice = model.CostPrice;
            product.Weight = model.Weight;
            product.Length = model.Length;
            product.Width = model.Width;
            product.Height = model.Height;
            product.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(_mapper.Map<ProductViewModel>(product));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId.Value);

            if (product == null)
            {
                return NotFound();
            }

            // Check if product has order items
            var hasOrderItems = await _context.OrderItems
                .AnyAsync(o => o.ProductId == id);

            if (hasOrderItems)
            {
                // Soft delete instead of hard delete
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Bu ürün sipariş kayıtlarında bulunduğu için pasif duruma alındı" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "TenantAdmin,TenantManager,TenantStaff")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId.Value && p.IsActive && p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<ProductViewModel>>(products));
        }

        private bool ProductExists(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            return _context.Products.Any(p => p.Id == id && p.TenantId == tenantId.Value);
        }
    }
} 