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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IMapper _mapper;

        public CategoriesController(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IMapper mapper)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.TenantId == tenantId.Value && c.IsActive)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<CategoryViewModel>>(categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoryViewModel>(category));
        }

        [HttpPost]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryViewModel model)
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

            // Validate parent category if provided
            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == model.ParentCategoryId.Value && c.TenantId == tenantId.Value);

                if (parentCategory == null)
                {
                    return BadRequest("Geçersiz üst kategori");
                }
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                ParentCategoryId = model.ParentCategoryId,
                TenantId = tenantId.Value,
                CreatedAt = DateTime.UtcNow,
                IsActive = model.IsActive
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, _mapper.Map<CategoryViewModel>(category));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryViewModel model)
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

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (category == null)
            {
                return NotFound();
            }

            // Validate parent category if provided
            if (model.ParentCategoryId.HasValue)
            {
                // Prevent circular reference
                if (model.ParentCategoryId.Value == id)
                {
                    return BadRequest("Kategori kendisini üst kategori olarak alamaz");
                }

                var parentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == model.ParentCategoryId.Value && c.TenantId == tenantId.Value);

                if (parentCategory == null)
                {
                    return BadRequest("Geçersiz üst kategori");
                }
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.ParentCategoryId = model.ParentCategoryId;
            category.IsActive = model.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(_mapper.Map<CategoryViewModel>(category));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (category == null)
            {
                return NotFound();
            }

            // Check if category has products
            var hasProducts = await _context.Products
                .AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                return BadRequest("Bu kategoriye bağlı ürünler bulunduğu için silinemez");
            }

            // Check if category has subcategories
            var hasSubcategories = await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == id);

            if (hasSubcategories)
            {
                return BadRequest("Bu kategorinin alt kategorileri bulunduğu için silinemez");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            return _context.Categories.Any(c => c.Id == id && c.TenantId == tenantId.Value);
        }
    }
} 