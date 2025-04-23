namespace OnlineTicaretOtomasyonu.ViewModels.Reports
{
    public class InventoryReportViewModel
    {
        // Summary
        public int TotalProducts { get; set; }
        public decimal TotalStockValue { get; set; }

        // Report items
        public List<InventoryReportItemViewModel> Items { get; set; } = new List<InventoryReportItemViewModel>();
    }

    public class InventoryReportItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string SKU { get; set; }
        public int StockQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal StockValue { get; set; }
    }
} 