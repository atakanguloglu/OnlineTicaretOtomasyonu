using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels.Pages
{
    // Sayfaların temel düzeni için kullanılacak sınıf
    public class PageStructure
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PageId { get; set; }
        public bool RequiresAuth { get; set; } = true;
        public List<string> AllowedRoles { get; set; } = new List<string>();
        public List<PageComponent> Components { get; set; } = new List<PageComponent>();
    }

    // Sayfa bileşenlerinin temel sınıfı
    public class PageComponent
    {
        public string ComponentType { get; set; }
        public string ComponentId { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    // Farklı bileşen tipleri

    // Tablo bileşeni
    public class TableComponent : PageComponent
    {
        public TableComponent()
        {
            ComponentType = "Table";
        }

        public string ApiEndpoint { get; set; }
        public bool Pagination { get; set; } = true;
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
        public List<string> Actions { get; set; } = new List<string>();
    }

    public class TableColumn
    {
        public string Field { get; set; }
        public string Title { get; set; }
        public string Type { get; set; } = "text";
        public bool Sortable { get; set; } = true;
        public bool Filterable { get; set; } = false;
    }

    // Form bileşeni
    public class FormComponent : PageComponent
    {
        public FormComponent()
        {
            ComponentType = "Form";
        }

        public string ApiEndpoint { get; set; }
        public string Method { get; set; } = "POST";
        public List<FormField> Fields { get; set; } = new List<FormField>();
        public string SubmitButtonText { get; set; } = "Kaydet";
    }

    public class FormField
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; } = "text";
        public bool Required { get; set; } = false;
        public string Placeholder { get; set; }
        public string Validation { get; set; }
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    // Kart bileşeni
    public class CardComponent : PageComponent
    {
        public CardComponent()
        {
            ComponentType = "Card";
        }

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public List<PageComponent> Children { get; set; } = new List<PageComponent>();
    }

    // Grafik bileşeni
    public class ChartComponent : PageComponent
    {
        public ChartComponent()
        {
            ComponentType = "Chart";
        }

        public string ChartType { get; set; } = "bar";
        public string ApiEndpoint { get; set; }
        public string Title { get; set; }
        public Dictionary<string, object> ChartOptions { get; set; } = new Dictionary<string, object>();
    }
} 