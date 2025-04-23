using System.ComponentModel.DataAnnotations;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class OrderViewModel
    {
        public int? Id { get; set; }

        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "Sipariş tarihi gereklidir")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Sipariş durumu gereklidir")]
        [StringLength(20, ErrorMessage = "Durum en fazla 20 karakter olabilir")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Ödeme durumu gereklidir")]
        [StringLength(20, ErrorMessage = "Ödeme durumu en fazla 20 karakter olabilir")]
        public string PaymentStatus { get; set; }

        [StringLength(20, ErrorMessage = "Ödeme yöntemi en fazla 20 karakter olabilir")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Toplam tutar gereklidir")]
        [Range(0, 9999999.99, ErrorMessage = "Toplam tutar geçerli bir değer olmalıdır")]
        public decimal TotalAmount { get; set; }

        [Range(0, 9999999.99, ErrorMessage = "Vergi tutarı geçerli bir değer olmalıdır")]
        public decimal TaxAmount { get; set; }

        [Range(0, 9999999.99, ErrorMessage = "Kargo tutarı geçerli bir değer olmalıdır")]
        public decimal ShippingAmount { get; set; }

        [Range(0, 9999999.99, ErrorMessage = "İndirim tutarı geçerli bir değer olmalıdır")]
        public decimal DiscountAmount { get; set; }

        [StringLength(255, ErrorMessage = "Notlar en fazla 255 karakter olabilir")]
        public string Notes { get; set; }

        // Shipping information
        [StringLength(100, ErrorMessage = "Kargo alıcı adı en fazla 100 karakter olabilir")]
        public string ShippingName { get; set; }

        [StringLength(255, ErrorMessage = "Kargo adresi en fazla 255 karakter olabilir")]
        public string ShippingAddress { get; set; }

        [StringLength(50, ErrorMessage = "Kargo şehri en fazla 50 karakter olabilir")]
        public string ShippingCity { get; set; }

        [StringLength(50, ErrorMessage = "Kargo ülkesi en fazla 50 karakter olabilir")]
        public string ShippingCountry { get; set; }

        [StringLength(20, ErrorMessage = "Kargo posta kodu en fazla 20 karakter olabilir")]
        public string ShippingPostalCode { get; set; }

        [StringLength(20, ErrorMessage = "Kargo telefonu en fazla 20 karakter olabilir")]
        public string ShippingPhone { get; set; }

        [Required(ErrorMessage = "Müşteri seçimi gereklidir")]
        public int CustomerId { get; set; }

        // Read-only property for display
        public string CustomerName { get; set; }

        // Order items
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int? Id { get; set; }

        public int OrderId { get; set; }

        [Required(ErrorMessage = "Ürün seçimi gereklidir")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Birim fiyat gereklidir")]
        [Range(0.01, 9999999.99, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Miktar gereklidir")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Toplam fiyat gereklidir")]
        [Range(0.01, 9999999.99, ErrorMessage = "Toplam fiyat 0'dan büyük olmalıdır")]
        public decimal TotalPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Vergi oranı 0-100 arasında olmalıdır")]
        public decimal TaxRate { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal DiscountAmount { get; set; }
    }
} 