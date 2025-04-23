using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Data;
using OnlineTicaretOtomasyonu.Models;

namespace OnlineTicaretOtomasyonu.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants.ToListAsync();
        }

        public async Task<Tenant> GetTenantByIdAsync(Guid id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task<Tenant> GetTenantBySlugAsync(string slug)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Slug == slug);
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            tenant.Id = Guid.NewGuid();
            tenant.CreatedAt = DateTime.UtcNow;
            tenant.IsActive = true;

            // Ensure the slug is unique
            tenant.Slug = await GenerateUniqueSlugAsync(tenant.Name);

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return tenant;
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant tenant)
        {
            var existingTenant = await _context.Tenants.FindAsync(tenant.Id);
            
            if (existingTenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenant.Id} not found.");
            }

            // Update properties
            existingTenant.Name = tenant.Name;
            existingTenant.Description = tenant.Description;
            existingTenant.ContactEmail = tenant.ContactEmail;
            existingTenant.ContactPhone = tenant.ContactPhone;
            existingTenant.Address = tenant.Address;
            existingTenant.City = tenant.City;
            existingTenant.Country = tenant.Country;
            existingTenant.PostalCode = tenant.PostalCode;
            existingTenant.CompanyName = tenant.CompanyName;
            existingTenant.TaxNumber = tenant.TaxNumber;
            existingTenant.IsActive = tenant.IsActive;
            existingTenant.UpdatedAt = DateTime.UtcNow;

            // Only update slug if it has changed and ensure it's unique
            if (existingTenant.Slug != tenant.Slug)
            {
                existingTenant.Slug = await GenerateUniqueSlugAsync(tenant.Name, tenant.Id);
            }

            await _context.SaveChangesAsync();
            return existingTenant;
        }

        public async Task<bool> DeleteTenantAsync(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            
            if (tenant == null)
            {
                return false;
            }

            // Soft delete by setting IsActive to false
            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTenantActiveAsync(Guid id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            return tenant?.IsActive ?? false;
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, Guid? excludeTenantId = null)
        {
            // Convert name to slug format
            var slug = name.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("@", "-at-");

            // Check if slug is unique
            var slugExists = await _context.Tenants
                .Where(t => t.Slug == slug)
                .Where(t => excludeTenantId == null || t.Id != excludeTenantId)
                .AnyAsync();

            if (!slugExists)
            {
                return slug;
            }

            // If not unique, append a number
            int counter = 1;
            string newSlug;
            
            do
            {
                newSlug = $"{slug}-{counter}";
                slugExists = await _context.Tenants
                    .Where(t => t.Slug == newSlug)
                    .Where(t => excludeTenantId == null || t.Id != excludeTenantId)
                    .AnyAsync();
                
                counter++;
            } while (slugExists);

            return newSlug;
        }
    }
} 