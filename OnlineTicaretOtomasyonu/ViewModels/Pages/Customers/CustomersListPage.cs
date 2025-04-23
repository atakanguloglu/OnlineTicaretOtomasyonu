using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels.Pages.Customers
{
    public class CustomersListPage
    {
        public static PageStructure GetStructure()
        {
            return new PageStructure
            {
                Title = "Müşteriler",
                Description = "Müşteri Listesi ve Yönetimi",
                PageId = "customers-list",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager", "TenantStaff" },
                Components = new List<PageComponent>
                {
                    // Üst Taraf Filtreler
                    new CardComponent
                    {
                        ComponentId = "customers-filters",
                        Title = "Filtreler",
                        Children = new List<PageComponent>
                        {
                            new FormComponent
                            {
                                ComponentId = "customers-filter-form",
                                Method = "GET",
                                Fields = new List<FormField>
                                {
                                    new FormField
                                    {
                                        Name = "search",
                                        Label = "Arama",
                                        Type = "text",
                                        Placeholder = "Müşteri adı, e-posta veya telefon"
                                    },
                                    new FormField
                                    {
                                        Name = "customerType",
                                        Label = "Müşteri Tipi",
                                        Type = "select",
                                        Options = new Dictionary<string, object>
                                        {
                                            { "ApiEndpoint", "/api/customers/types" },
                                            { "ValueField", "" },
                                            { "TextField", "" },
                                            { "AllowEmpty", true },
                                            { "EmptyText", "Tümü" }
                                        }
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
                    
                    // Müşteri Listesi
                    new CardComponent
                    {
                        ComponentId = "customers-list-card",
                        Title = "Müşteri Listesi",
                        Properties = new Dictionary<string, object>
                        {
                            { "HeaderActions", new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        { "Label", "Yeni Müşteri" },
                                        { "Icon", "plus" },
                                        { "Action", "navigate" },
                                        { "ActionTarget", "/customers/new" }
                                    }
                                }
                            }
                        },
                        Children = new List<PageComponent>
                        {
                            new TableComponent
                            {
                                ComponentId = "customers-table",
                                ApiEndpoint = "/api/customers",
                                Pagination = true,
                                Columns = new List<TableColumn>
                                {
                                    new TableColumn 
                                    { 
                                        Field = "id", 
                                        Title = "ID", 
                                        Sortable = true,
                                        Type = "number"
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "fullName", 
                                        Title = "Müşteri Adı", 
                                        Sortable = true,
                                        Filterable = true
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "email", 
                                        Title = "E-posta", 
                                        Sortable = true 
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "phone", 
                                        Title = "Telefon" 
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "companyName", 
                                        Title = "Firma" 
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "customerType", 
                                        Title = "Müşteri Tipi",
                                        Type = "badge"
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "isActive", 
                                        Title = "Durum", 
                                        Type = "boolean" 
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