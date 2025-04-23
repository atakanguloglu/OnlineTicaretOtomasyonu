using System.ComponentModel.DataAnnotations;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class TenantViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Firma/Kiracı adı gereklidir")]
        [StringLength(100, ErrorMessage = "Firma/Kiracı adı en fazla 100 karakter olabilir")]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = "Açıklama en fazla 255 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "İletişim e-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir")]
        public string ContactEmail { get; set; }

        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        public string ContactPhone { get; set; }

        [StringLength(255, ErrorMessage = "Adres en fazla 255 karakter olabilir")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olabilir")]
        public string City { get; set; }

        [StringLength(50, ErrorMessage = "Ülke en fazla 50 karakter olabilir")]
        public string Country { get; set; }

        [StringLength(20, ErrorMessage = "Posta kodu en fazla 20 karakter olabilir")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Firma adı gereklidir")]
        [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir")]
        public string CompanyName { get; set; }

        [StringLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir")]
        public string TaxNumber { get; set; }

        public bool IsActive { get; set; } = true;

        // Admin kullanıcı bilgileri (yeni tenant oluşturulurken)
        public TenantAdminViewModel AdminUser { get; set; }
    }

    public class TenantAdminViewModel
    {
        [Required(ErrorMessage = "Admin adı gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Admin soyadı gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Admin e-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }
    }
} 