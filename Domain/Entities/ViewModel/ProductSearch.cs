namespace Entities.ViewModel;
public class ProductSearch
{
    public string? keyword { get; set; }
    public int? categoryId { get; set; }
    public decimal? minPrice { get; set; }
    public decimal? maxPrice { get; set; }
    public bool? isFeatured { get; set; }
    public bool? isActive { get; set; }

 
    public bool? inStock { get; set; }
    public bool? onSale { get; set; }
    public bool? featured { get; set; }
    public string? sortBy { get; set; }
    public string? brand { get; set; }

    public int currentPage { get; set; } = 1;
    public int pageSize { get; set; } = 12;
    public int skip { get; set; } = 0;
}