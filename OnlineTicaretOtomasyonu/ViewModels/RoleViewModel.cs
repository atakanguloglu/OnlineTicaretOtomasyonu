using System.ComponentModel.DataAnnotations;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }

    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Rol adı gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı 2-50 karakter arasında olmalıdır")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Rol adı sadece harf, rakam, alt çizgi ve kısa çizgi içerebilir")]
        public string Name { get; set; }
    }

    public class UpdateRoleViewModel
    {
        [Required(ErrorMessage = "Rol adı gereklidir")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Rol adı 2-50 karakter arasında olmalıdır")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Rol adı sadece harf, rakam, alt çizgi ve kısa çizgi içerebilir")]
        public string Name { get; set; }
    }
} 