using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels.Pages.Users
{
    // Model sınıflarının tanımlarını ekliyoruz
    public class PageStructure
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PageId { get; set; }
        public bool RequiresAuth { get; set; }
        public List<string> AllowedRoles { get; set; }
        public List<PageComponent> Components { get; set; }
    }

    public class PageComponent
    {
        // Temel bileşen özellikleri
        public string ComponentId { get; set; }
    }

    public class CardComponent : PageComponent
    {
        public string Title { get; set; }
        public List<PageComponent> Children { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class FormComponent : PageComponent
    {
        public string Method { get; set; }
        public List<FormField> Fields { get; set; }
        public string SubmitButtonText { get; set; }
    }

    public class FormField
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string Placeholder { get; set; }
        public Dictionary<string, object> Options { get; set; }
    }

    public class TableComponent : PageComponent
    {
        public string ApiEndpoint { get; set; }
        public bool Pagination { get; set; }
        public List<TableColumn> Columns { get; set; }
        public List<string> Actions { get; set; }
    }

    public class TableColumn
    {
        public string Field { get; set; }
        public string Title { get; set; }
        public bool Sortable { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class UsersListPage
    {
        public static PageStructure GetStructure()
        {
            return new PageStructure
            {
                Title = "Kullanıcılar",
                Description = "Sistem Kullanıcıları Yönetimi",
                PageId = "users-list",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "SuperAdmin", "TenantAdmin" },
                Components = new List<PageComponent>
                {
                    // Üst Taraf Filtreler
                    new CardComponent
                    {
                        ComponentId = "users-filters",
                        Title = "Filtreler",
                        Children = new List<PageComponent>
                        {
                            new FormComponent
                            {
                                ComponentId = "users-filter-form",
                                Method = "GET",
                                Fields = new List<FormField>
                                {
                                    new FormField
                                    {
                                        Name = "search",
                                        Label = "Arama",
                                        Type = "text",
                                        Placeholder = "Kullanıcı adı, e-posta veya ad soyad"
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
                    
                    // Kullanıcı Listesi
                    new CardComponent
                    {
                        ComponentId = "users-list-card",
                        Title = "Kullanıcı Listesi",
                        Properties = new Dictionary<string, object>
                        {
                            { "HeaderActions", new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        { "Label", "Yeni Kullanıcı" },
                                        { "Icon", "plus" },
                                        { "Action", "navigate" },
                                        { "ActionTarget", "/users/new" }
                                    }
                                }
                            }
                        },
                        Children = new List<PageComponent>
                        {
                            new TableComponent
                            {
                                ComponentId = "users-table",
                                ApiEndpoint = "/api/users",
                                Pagination = true,
                                Columns = new List<TableColumn>
                                {
                                    new TableColumn
                                    {
                                        Field = "fullName",
                                        Title = "Ad Soyad",
                                        Sortable = true
                                    },
                                    new TableColumn
                                    {
                                        Field = "email",
                                        Title = "E-posta",
                                        Sortable = true
                                    },
                                    new TableColumn
                                    {
                                        Field = "phoneNumber",
                                        Title = "Telefon"
                                    },
                                    new TableColumn
                                    {
                                        Field = "roles",
                                        Title = "Roller",
                                        Type = "badges"
                                    },
                                    new TableColumn
                                    {
                                        Field = "tenantName",
                                        Title = "Tenant",
                                        Type = "text",
                                        Properties = new Dictionary<string, object>
                                        {
                                            { "SuperAdminOnly", true } // Sadece SuperAdmin görebilir
                                        }
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