namespace OnlineTicaretOtomasyonu.ViewModels.Reports
{
    public class SalesReportViewModel
    {
        // Report parameters
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupBy { get; set; } = "day";

        // Summary
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }

        // Report items
        public List<SalesReportItemViewModel> Items { get; set; } = new List<SalesReportItemViewModel>();
    }

    public class SalesReportItemViewModel
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public int OrderCount { get; set; }
    }
} 