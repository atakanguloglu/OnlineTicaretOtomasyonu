using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.ViewModels
{
    public class PagedResultViewModel<T> where T : class
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public PagedResultViewModel(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
        }

        public static PagedResultViewModel<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResultViewModel<T>(items, totalCount, pageNumber, pageSize);
        }
    }
} 