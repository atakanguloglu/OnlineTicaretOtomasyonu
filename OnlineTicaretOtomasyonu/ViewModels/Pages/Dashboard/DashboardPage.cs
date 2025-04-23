using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels.Pages.Dashboard
{
    public class DashboardPage
    {
        public static PageStructure GetStructure()
        {
            return new PageStructure
            {
                Title = "Dashboard",
                Description = "Ticaret Otomasyonu Kontrol Paneli",
                PageId = "dashboard",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager" },
                Components = new List<PageComponent>
                {
                    // Özet Kartları Row
                    new CardComponent
                    {
                        ComponentId = "summary-cards",
                        Title = "Özet Bilgiler",
                        Children = new List<PageComponent>
                        {
                            new CardComponent
                            {
                                ComponentId = "card-sales-today",
                                Title = "Bugünkü Satış",
                                Properties = new Dictionary<string, object>
                                {
                                    { "ApiDataPath", "salesToday" },
                                    { "Icon", "shopping-cart" },
                                    { "Color", "success" },
                                    { "IsCurrency", true }
                                }
                            },
                            new CardComponent
                            {
                                ComponentId = "card-sales-month",
                                Title = "Bu Ay Satış",
                                Properties = new Dictionary<string, object>
                                {
                                    { "ApiDataPath", "salesThisMonth" },
                                    { "Icon", "calendar" },
                                    { "Color", "primary" },
                                    { "IsCurrency", true },
                                    { "Comparison", new Dictionary<string, object>
                                        {
                                            { "Label", "Geçen Aya Göre" },
                                            { "Value", "monthOverMonthChange" },
                                            { "ValueFormat", "percent" }
                                        }
                                    }
                                }
                            },
                            new CardComponent
                            {
                                ComponentId = "card-orders-today",
                                Title = "Bugünkü Siparişler",
                                Properties = new Dictionary<string, object>
                                {
                                    { "ApiDataPath", "newOrdersToday" },
                                    { "Icon", "package" },
                                    { "Color", "warning" }
                                }
                            },
                            new CardComponent
                            {
                                ComponentId = "card-customers",
                                Title = "Toplam Müşteri",
                                Properties = new Dictionary<string, object>
                                {
                                    { "ApiDataPath", "totalCustomers" },
                                    { "Icon", "users" },
                                    { "Color", "info" }
                                }
                            },
                        }
                    },
                    
                    // Grafikler Row
                    new CardComponent
                    {
                        ComponentId = "charts-row",
                        Title = "Satış Grafikleri",
                        Children = new List<PageComponent>
                        {
                            new ChartComponent
                            {
                                ComponentId = "chart-sales-trend",
                                Title = "Satış Trendi",
                                ChartType = "line",
                                ApiEndpoint = "/api/reports/sales?groupBy=day",
                                Properties = new Dictionary<string, object>
                                {
                                    { "XAxisField", "date" },
                                    { "YAxisField", "totalAmount" },
                                    { "DateFormat", "DD MMM" }
                                }
                            },
                            new ChartComponent
                            {
                                ComponentId = "chart-sales-by-category",
                                Title = "Kategorilere Göre Satışlar",
                                ChartType = "doughnut",
                                Properties = new Dictionary<string, object>
                                {
                                    { "ApiDataPath", "salesByCategory" },
                                    { "LabelField", "categoryName" },
                                    { "ValueField", "totalAmount" }
                                }
                            }
                        }
                    },
                    
                    // Son Siparişler Tablosu
                    new TableComponent
                    {
                        ComponentId = "recent-orders-table",
                        Title = "Son Siparişler",
                        ApiEndpoint = "/api/orders?pageSize=5",
                        Pagination = false,
                        Columns = new List<TableColumn>
                        {
                            new TableColumn { Field = "orderNumber", Title = "Sipariş No" },
                            new TableColumn { Field = "orderDate", Title = "Tarih", Type = "date" },
                            new TableColumn { Field = "customerName", Title = "Müşteri" },
                            new TableColumn { Field = "totalAmount", Title = "Tutar", Type = "currency" },
                            new TableColumn { Field = "status", Title = "Durum", Type = "badge" }
                        },
                        Actions = new List<string> { "view" }
                    },
                    
                    // En Çok Satan Ürünler Tablosu
                    new TableComponent
                    {
                        ComponentId = "top-selling-products",
                        Title = "En Çok Satan Ürünler",
                        Pagination = false,
                        Properties = new Dictionary<string, object>
                        {
                            { "ApiDataPath", "topSellingProducts" }
                        },
                        Columns = new List<TableColumn>
                        {
                            new TableColumn { Field = "productName", Title = "Ürün" },
                            new TableColumn { Field = "totalQuantity", Title = "Satış Adedi" },
                            new TableColumn { Field = "totalAmount", Title = "Toplam Tutar", Type = "currency" }
                        },
                        Actions = new List<string> { "view" }
                    }
                }
            };
        }
    }
} 