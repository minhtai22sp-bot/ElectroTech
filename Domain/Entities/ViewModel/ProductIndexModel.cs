namespace Entities.ViewModel;

public class ProductIndexModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Code { get; set; }
    public string Brand { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Stock { get; set; }
    public string ThumbnailUrl { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}