namespace OnlineTicaretOtomasyonu.ViewModels.Reports
{
    public class DashboardViewModel
    {
        // Sales metrics
        public decimal SalesToday { get; set; }
        public decimal SalesThisMonth { get; set; }
        public decimal SalesLastMonth { get; set; }
        public decimal MonthOverMonthChange { get; set; }

        // Order metrics
        public int NewOrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }

        // Customer metrics
        public int TotalCustomers { get; set; }

        // Product metrics
        public int LowStockProducts { get; set; }

        // Top selling products
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new List<TopSellingProductViewModel>();

        // Sales by category
        public List<SalesByCategoryViewModel> SalesByCategory { get; set; } = new List<SalesByCategoryViewModel>();

        // Recent orders
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
    }

    public class TopSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SalesByCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
} 