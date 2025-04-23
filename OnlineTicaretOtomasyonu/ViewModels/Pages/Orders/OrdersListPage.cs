using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels.Pages.Orders
{
    public class OrdersListPage
    {
        public static PageStructure GetStructure()
        {
            return new PageStructure
            {
                Title = "Siparişler",
                Description = "Sipariş Listesi ve Yönetimi",
                PageId = "orders-list",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager", "TenantStaff" },
                Components = new List<PageComponent>
                {
                    // Üst Taraf Filtreler
                    new CardComponent
                    {
                        ComponentId = "orders-filters",
                        Title = "Filtreler",
                        Children = new List<PageComponent>
                        {
                            new FormComponent
                            {
                                ComponentId = "orders-filter-form",
                                Method = "GET",
                                Fields = new List<FormField>
                                {
                                    new FormField
                                    {
                                        Name = "search",
                                        Label = "Arama",
                                        Type = "text",
                                        Placeholder = "Sipariş no, müşteri adı veya e-posta"
                                    },
                                    new FormField
                                    {
                                        Name = "status",
                                        Label = "Sipariş Durumu",
                                        Type = "select",
                                        Options = new Dictionary<string, object>
                                        {
                                            { "ApiEndpoint", "/api/orders/statuses" },
                                            { "AllowEmpty", true },
                                            { "EmptyText", "Tümü" }
                                        }
                                    },
                                    new FormField
                                    {
                                        Name = "paymentStatus",
                                        Label = "Ödeme Durumu",
                                        Type = "select",
                                        Options = new Dictionary<string, object>
                                        {
                                            { "ApiEndpoint", "/api/orders/payment-statuses" },
                                            { "AllowEmpty", true },
                                            { "EmptyText", "Tümü" }
                                        }
                                    },
                                    new FormField
                                    {
                                        Name = "fromDate",
                                        Label = "Başlangıç Tarihi",
                                        Type = "date"
                                    },
                                    new FormField
                                    {
                                        Name = "toDate",
                                        Label = "Bitiş Tarihi",
                                        Type = "date"
                                    }
                                },
                                SubmitButtonText = "Filtrele"
                            }
                        },
                        Properties = new Dictionary<string, object>
                        {
                            { "Collapsible", true },
                            { "DefaultCollapsed", true }
                        }
                    },

                    // Sipariş Listesi
                    new CardComponent
                    {
                        ComponentId = "orders-list-card",
                        Title = "Sipariş Listesi",
                        Properties = new Dictionary<string, object>
                        {
                            { "HeaderActions", new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        { "Label", "Yeni Sipariş" },
                                        { "Icon", "plus" },
                                        { "Action", "navigate" },
                                        { "ActionTarget", "/orders/new" }
                                    }
                                }
                            }
                        },
                        Children = new List<PageComponent>
                        {
                            new TableComponent
                            {
                                ComponentId = "orders-table",
                                ApiEndpoint = "/api/orders",
                                Pagination = true,
                                Columns = new List<TableColumn>
                                {
                                    new TableColumn
                                    {
                                        Field = "orderNumber",
                                        Title = "Sipariş No",
                                        Sortable = true
                                    },
                                    new TableColumn
                                    {
                                        Field = "orderDate",
                                        Title = "Tarih",
                                        Sortable = true,
                                        Type = "date"
                                    },
                                    new TableColumn
                                    {
                                        Field = "customerName",
                                        Title = "Müşteri",
                                        Sortable = true
                                    },
                                    new TableColumn
                                    {
                                        Field = "totalAmount",
                                        Title = "Tutar",
                                        Sortable = true,
                                        Type = "currency"
                                    },
                                    new TableColumn
                                    {
                                        Field = "status",
                                        Title = "Sipariş Durumu",
                                        Type = "badge"
                                    },
                                    new TableColumn
                                    {
                                        Field = "paymentStatus",
                                        Title = "Ödeme Durumu",
                                        Type = "badge"
                                    }
                                },
                                Actions = new List<string> { "view", "edit", "delete" }
                            }
                        }
                    }
                }
            };
        }
    }
}