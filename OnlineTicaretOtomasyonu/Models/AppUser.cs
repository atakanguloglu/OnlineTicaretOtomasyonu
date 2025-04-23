using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicaretOtomasyonu.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Tenant relationship
        public Guid? TenantId { get; set; }
        
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }
        
        // User role within tenant (Admin, Manager, Staff, etc.)
        public string TenantRole { get; set; }
        
        // Profile picture or avatar
        public string ProfilePictureUrl { get; set; }
        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
} 