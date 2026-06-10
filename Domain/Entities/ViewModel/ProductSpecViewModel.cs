using System.ComponentModel.DataAnnotations;

namespace Entities.ViewModel;

public class ProductSpecViewModel
{
    public int Id { get; set; }
    public string GroupName { get; set; } = "";
    public string SpecKey { get; set; } = "";
    public string SpecValue { get; set; } = "";
    public int DisplayOrder { get; set; }
}

public class CreateProductViewModel
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
    public int CategoryId { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    //  Dùng string để tránh culture binding fail
    [Required(ErrorMessage = "Vui lòng nhập giá bán")]
    public string PriceInput { get; set; } = "";

    public string? OriginalPriceInput { get; set; }

    // Không dùng asp-for — parse thủ công
    public decimal GetPrice() =>
        decimal.TryParse(PriceInput,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : 0;

    public decimal? GetOriginalPrice() =>
        string.IsNullOrWhiteSpace(OriginalPriceInput) ? null :
        decimal.TryParse(OriginalPriceInput,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : null;

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    public List<ProductSpecViewModel> Specs { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
}

//  Chỉ định nghĩa MỘT lần duy nhất ở đây
public class EditProductViewModel : CreateProductViewModel
{
    public int Id { get; set; }
}