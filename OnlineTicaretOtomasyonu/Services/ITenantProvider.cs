using OnlineTicaretOtomasyonu.Models;

namespace OnlineTicaretOtomasyonu.Services
{
    public interface ITenantProvider
    {
        Guid? GetTenantId();
        void SetTenantId(Guid tenantId);
        Task<Tenant> GetCurrentTenantAsync();
    }
} 