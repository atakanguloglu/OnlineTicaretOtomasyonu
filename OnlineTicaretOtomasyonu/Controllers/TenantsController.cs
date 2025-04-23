using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public TenantsController(
            ITenantService tenantService,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _tenantService = tenantService;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(tenants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenant(Guid id)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id);
            
            if (tenant == null)
            {
                return NotFound();
            }

            return Ok(tenant);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] TenantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create tenant
            var tenant = new Tenant
            {
                Name = model.Name,
                Description = model.Description,
                ContactEmail = model.ContactEmail,
                ContactPhone = model.ContactPhone,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode,
                CompanyName = model.CompanyName,
                TaxNumber = model.TaxNumber,
                IsActive = model.IsActive
            };

            var createdTenant = await _tenantService.CreateTenantAsync(tenant);

            // Create tenant admin user if provided
            if (model.AdminUser != null)
            {
                var adminUser = new AppUser
                {
                    UserName = model.AdminUser.Email,
                    Email = model.AdminUser.Email,
                    FirstName = model.AdminUser.FirstName,
                    LastName = model.AdminUser.LastName,
                    TenantId = createdTenant.Id,
                    TenantRole = "TenantAdmin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(adminUser, model.AdminUser.Password);
                
                if (!result.Succeeded)
                {
                    // If user creation fails, delete the tenant as well
                    await _tenantService.DeleteTenantAsync(createdTenant.Id);
                    return BadRequest(result.Errors);
                }

                // Assign TenantAdmin role
                await _userManager.AddToRoleAsync(adminUser, "TenantAdmin");
            }

            return CreatedAtAction(nameof(GetTenant), new { id = createdTenant.Id }, createdTenant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] TenantViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTenant = await _tenantService.GetTenantByIdAsync(id);
            
            if (existingTenant == null)
            {
                return NotFound();
            }

            // Update tenant properties
            existingTenant.Name = model.Name;
            existingTenant.Description = model.Description;
            existingTenant.ContactEmail = model.ContactEmail;
            existingTenant.ContactPhone = model.ContactPhone;
            existingTenant.Address = model.Address;
            existingTenant.City = model.City;
            existingTenant.Country = model.Country;
            existingTenant.PostalCode = model.PostalCode;
            existingTenant.CompanyName = model.CompanyName;
            existingTenant.TaxNumber = model.TaxNumber;
            existingTenant.IsActive = model.IsActive;

            try
            {
                var updatedTenant = await _tenantService.UpdateTenantAsync(existingTenant);
                return Ok(updatedTenant);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(Guid id)
        {
            var result = await _tenantService.DeleteTenantAsync(id);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
} 