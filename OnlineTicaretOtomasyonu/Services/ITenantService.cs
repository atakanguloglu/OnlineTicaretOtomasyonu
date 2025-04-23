using OnlineTicaretOtomasyonu.Models;

namespace OnlineTicaretOtomasyonu.Services
{
    public interface ITenantService
    {
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<Tenant> GetTenantByIdAsync(Guid id);
        Task<Tenant> GetTenantBySlugAsync(string slug);
        Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task<Tenant> UpdateTenantAsync(Tenant tenant);
        Task<bool> DeleteTenantAsync(Guid id);
        Task<bool> IsTenantActiveAsync(Guid id);
    }
} 