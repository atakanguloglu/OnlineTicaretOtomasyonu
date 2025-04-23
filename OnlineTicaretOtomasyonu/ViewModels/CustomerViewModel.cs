using System.ComponentModel.DataAnnotations;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class CustomerViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta adresi en fazla 100 karakter olabilir")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir")]
        public string Phone { get; set; }

        [StringLength(255, ErrorMessage = "Adres en fazla 255 karakter olabilir")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olabilir")]
        public string City { get; set; }

        [StringLength(50, ErrorMessage = "Ülke en fazla 50 karakter olabilir")]
        public string Country { get; set; }

        [StringLength(20, ErrorMessage = "Posta kodu en fazla 20 karakter olabilir")]
        public string PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "Firma adı en fazla 50 karakter olabilir")]
        public string CompanyName { get; set; }

        [StringLength(20, ErrorMessage = "Vergi numarası en fazla 20 karakter olabilir")]
        public string TaxNumber { get; set; }

        [Required(ErrorMessage = "Müşteri tipi gereklidir")]
        [StringLength(20, ErrorMessage = "Müşteri tipi en fazla 20 karakter olabilir")]
        public string CustomerType { get; set; }

        public bool IsActive { get; set; } = true;

        // Read-only property
        public string FullName => $"{FirstName} {LastName}";

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 