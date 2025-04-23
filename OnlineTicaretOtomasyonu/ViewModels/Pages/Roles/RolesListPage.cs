using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels.Pages.Roles
{
    public class RolesListPage
    {
        public static PageStructure GetStructure()
        {
            return new PageStructure
            {
                Title = "Roller",
                Description = "Sistem Rolleri Yönetimi",
                PageId = "roles-list",
                RequiresAuth = true,
                AllowedRoles = new List<string> { "SuperAdmin", "TenantAdmin" },
                Components = new List<PageComponent>
                {
                    // Üst Taraf Filtreler
                    new CardComponent
                    {
                        ComponentId = "roles-filters",
                        Title = "Filtreler",
                        Children = new List<PageComponent>
                        {
                            new FormComponent
                            {
                                ComponentId = "roles-filter-form",
                                Method = "GET",
                                Fields = new List<FormField>
                                {
                                    new FormField
                                    {
                                        Name = "search",
                                        Label = "Arama",
                                        Type = "text",
                                        Placeholder = "Rol adı"
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
                    
                    // Rol Listesi
                    new CardComponent
                    {
                        ComponentId = "roles-list-card",
                        Title = "Rol Listesi",
                        Properties = new Dictionary<string, object>
                        {
                            { "HeaderActions", new List<Dictionary<string, object>>
                                {
                                    new Dictionary<string, object>
                                    {
                                        { "Label", "Yeni Rol" },
                                        { "Icon", "plus" },
                                        { "Action", "navigate" },
                                        { "ActionTarget", "/roles/new" },
                                        { "Roles", new List<string> { "SuperAdmin" } } // Sadece SuperAdmin yeni rol oluşturabilir
                                    }
                                }
                            }
                        },
                        Children = new List<PageComponent>
                        {
                            new TableComponent
                            {
                                ComponentId = "roles-table",
                                ApiEndpoint = "/api/roles",
                                Pagination = true,
                                Columns = new List<TableColumn>
                                {
                                    new TableColumn 
                                    { 
                                        Field = "name", 
                                        Title = "Rol Adı", 
                                        Sortable = true
                                    },
                                    new TableColumn 
                                    { 
                                        Field = "normalizedName", 
                                        Title = "Normalized Ad"
                                    }
                                },
                                Actions = new List<string> 
                                { 
                                    "view", 
                                    "edit", 
                                    "delete", 
                                    new FormattedAction 
                                    { 
                                        Name = "users", 
                                        Label = "Kullanıcılar", 
                                        Icon = "users", 
                                        Action = "navigate", 
                                        Target = "/roles/{id}/users" 
                                    }.ToString() 
                                }
                            }
                        }
                    }
                }
            };
        }
    }

    public class FormattedAction
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public string Action { get; set; }
        public string Target { get; set; }

        public override string ToString()
        {
            return $"{Name}:{Label}:{Icon}:{Action}:{Target}";
        }
    }
} 