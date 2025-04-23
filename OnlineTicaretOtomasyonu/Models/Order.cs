using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineTicaretOtomasyonu.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        // Order status (Pending, Processing, Shipped, Delivered, Cancelled)
        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        // Payment status (Pending, Paid, Failed, Refunded)
        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; }

        // Payment method (Credit Card, PayPal, Bank Transfer, etc.)
        [StringLength(20)]
        public string PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [StringLength(255)]
        public string Notes { get; set; }

        // Shipping information
        [StringLength(100)]
        public string ShippingName { get; set; }

        [StringLength(255)]
        public string ShippingAddress { get; set; }

        [StringLength(50)]
        public string ShippingCity { get; set; }

        [StringLength(50)]
        public string ShippingCountry { get; set; }

        [StringLength(20)]
        public string ShippingPostalCode { get; set; }

        [StringLength(20)]
        public string ShippingPhone { get; set; }

        // Customer relationship
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        // Tenant relationship
        [Required]
        public Guid TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
} 