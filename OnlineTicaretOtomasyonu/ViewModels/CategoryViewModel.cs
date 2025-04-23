using System.ComponentModel.DataAnnotations;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class CategoryViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = "Açıklama en fazla 255 karakter olabilir")]
        public string Description { get; set; }

        public int? ParentCategoryId { get; set; }
        
        // Read-only property for display
        public string ParentCategoryName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 