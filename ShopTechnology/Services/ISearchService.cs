using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface ISearchService
    {
        Task<SearchResult> SearchProductsAsync(string query, SearchFilters filters, int page = 1, int pageSize = 12);
        Task<List<string>> GetSearchSuggestionsAsync(string query);
        Task<List<Product>> GetRelatedProductsAsync(int productId, int limit = 6);
        Task<List<Product>> GetPopularSearchesAsync(int limit = 10);
        Task IndexProductAsync(Product product);
        Task RemoveProductFromIndexAsync(int productId);
        Task RebuildSearchIndexAsync();
    }

    public class SearchResult
    {
        public List<Product> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<SearchFacet> Facets { get; set; } = new();
    }

    public class SearchFilters
    {
        public List<int> CategoryIds { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<int> BrandIds { get; set; } = new();
        public double? MinRating { get; set; }
        public bool? InStock { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsNew { get; set; }
        public string SortBy { get; set; } = "relevance";
        public string SortOrder { get; set; } = "desc";
    }

    public class SearchFacet
    {
        public string Name { get; set; } = string.Empty;
        public List<FacetValue> Values { get; set; } = new();
    }

    public class FacetValue
    {
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
