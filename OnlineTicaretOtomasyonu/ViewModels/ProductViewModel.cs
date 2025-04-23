using System.ComponentModel.DataAnnotations;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class ProductViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = "Açıklama en fazla 255 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, 9999999.99, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz")]
        public int StockQuantity { get; set; }

        [StringLength(50, ErrorMessage = "SKU en fazla 50 karakter olabilir")]
        public string SKU { get; set; }

        [StringLength(50, ErrorMessage = "Barkod en fazla 50 karakter olabilir")]
        public string Barcode { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        [StringLength(255, ErrorMessage = "Resim URL'si en fazla 255 karakter olabilir")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Kategori seçimi gereklidir")]
        public int CategoryId { get; set; }

        // Read-only property for display
        public string CategoryName { get; set; }

        [Range(0, 100, ErrorMessage = "Vergi oranı 0-100 arasında olmalıdır")]
        public decimal TaxRate { get; set; } = 0;

        [Required(ErrorMessage = "Maliyet fiyatı gereklidir")]
        [Range(0.01, 9999999.99, ErrorMessage = "Maliyet fiyatı 0'dan büyük olmalıdır")]
        public decimal CostPrice { get; set; }

        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 