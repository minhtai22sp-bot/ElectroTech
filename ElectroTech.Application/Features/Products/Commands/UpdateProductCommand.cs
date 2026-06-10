using AspNetCoreHero.Results;
using Entities.ViewModel;
using Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

// Alias để tránh conflict với namespace Products
using ProductSpecEntity = Entities.ProductSpec;
using ProductImageEntity = Entities.ProductImage;

namespace ElectroTech.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<IResult<int>>
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }

    public List<ProductSpecViewModel> Specs { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();

    public class UpdateProductCommandHandler
        : IRequestHandler<UpdateProductCommand, IResult<int>>
    {
        private readonly IProductRepository _repository;

        public UpdateProductCommandHandler(IProductRepository repository)
            => _repository = repository;

        public async Task<IResult<int>> Handle(
            UpdateProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _repository.GetById(command.Id);
                if (product is null)
                    return await Result<int>.FailAsync("Không tìm thấy sản phẩm.");

                product.Name = command.Name;
                product.Slug = GenerateSlug(command.Name);
                product.Code = command.Code ?? "";
                product.Brand = command.Brand ?? "";
                product.CategoryId = command.CategoryId;
                product.Price = command.Price;
                product.OriginalPrice = command.OriginalPrice;
                product.Stock = command.Stock;
                product.Description = command.Description ?? "";
                product.ThumbnailUrl = command.ThumbnailUrl ?? "";
                product.IsFeatured = command.IsFeatured;
                product.IsActive = command.IsActive;

                await _repository.UpdateAsync(product);

                // Lưu Specs
                var specs = command.Specs
                    .Where(s => !string.IsNullOrWhiteSpace(s.SpecKey)
                             && !string.IsNullOrWhiteSpace(s.SpecValue))
                    .Select((s, i) => new ProductSpecEntity
                    {
                        GroupName = s.GroupName ?? "",
                        SpecKey = s.SpecKey,
                        SpecValue = s.SpecValue,
                        DisplayOrder = i
                    }).ToList();

                await _repository.SaveSpecsAsync(product.Id, specs);

                // Lưu Images
                var images = command.ImageUrls
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select((u, i) => new ProductImageEntity
                    {
                        ImageUrl = u,
                        DisplayOrder = i,
                        IsPrimary = i == 0
                    }).ToList();

                await _repository.SaveImagesAsync(product.Id, images);

                return await Result<int>.SuccessAsync(
                    product.Id, "Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return await Result<int>.FailAsync($"Lỗi: {ex.Message}");
            }
        }

        private static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var map = new Dictionary<string, string>
            {
                {"à|á|ả|ã|ạ|ă|ắ|ặ|ằ|ẳ|ẵ|â|ấ|ầ|ẩ|ẫ|ậ", "a"},
                {"è|é|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ", "e"},
                {"ì|í|ỉ|ĩ|ị", "i"},
                {"ò|ó|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ", "o"},
                {"ù|ú|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự", "u"},
                {"ỳ|ý|ỷ|ỹ|ỵ", "y"},
                {"đ", "d"}
            };
            var slug = name.ToLower().Trim();
            foreach (var (p, r) in map)
                slug = Regex.Replace(slug, p, r);
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            return slug.Trim('-');
        }
    }
}