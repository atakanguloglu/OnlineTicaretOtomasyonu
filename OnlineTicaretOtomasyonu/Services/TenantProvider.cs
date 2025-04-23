using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Data;
using OnlineTicaretOtomasyonu.Models;
using System.Security.Claims;

namespace OnlineTicaretOtomasyonu.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private Guid? _currentTenantId;

        public TenantProvider(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Guid? GetTenantId()
        {
            if (_currentTenantId.HasValue)
            {
                return _currentTenantId;
            }

            // Try to get tenant from user claims
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = user.FindFirst("TenantId");
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out Guid tenantId))
                {
                    _currentTenantId = tenantId;
                    return _currentTenantId;
                }
            }

            // Try to get tenant from request header
            var tenantHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-TenantId"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tenantHeader) && Guid.TryParse(tenantHeader, out Guid headerTenantId))
            {
                _currentTenantId = headerTenantId;
                return _currentTenantId;
            }

            // Try to get tenant from subdomain
            var host = _httpContextAccessor.HttpContext?.Request.Host.Value;
            if (!string.IsNullOrEmpty(host))
            {
                var subdomain = host.Split('.').FirstOrDefault();
                if (!string.IsNullOrEmpty(subdomain) && subdomain != "www")
                {
                    // Look up tenant by subdomain (slug)
                    // This could be cached for better performance
                    var tenant = _context.Tenants
                        .FirstOrDefault(t => t.Slug == subdomain && t.IsActive);
                    
                    if (tenant != null)
                    {
                        _currentTenantId = tenant.Id;
                        return _currentTenantId;
                    }
                }
            }

            return null;
        }

        public void SetTenantId(Guid tenantId)
        {
            _currentTenantId = tenantId;
        }

        public async Task<Tenant> GetCurrentTenantAsync()
        {
            var tenantId = GetTenantId();
            if (tenantId.HasValue)
            {
                return await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantId.Value && t.IsActive);
            }
            
            return null;
        }
    }
} 