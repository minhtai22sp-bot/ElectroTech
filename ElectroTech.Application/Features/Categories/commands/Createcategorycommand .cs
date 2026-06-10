using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CategoryEntity = Entities.Categories;
namespace ElectroTech.Application.Features.Categories.commands
{
    public class CreateCategoryCommand : IRequest<IResult<int>>
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public class Handler : IRequestHandler<CreateCategoryCommand, IResult<int>>
        {
            private readonly ICategoryRepository _repo;
            public Handler(ICategoryRepository repo) => _repo = repo;

            public async Task<IResult<int>> Handle(
                CreateCategoryCommand cmd, CancellationToken ct)
            {
                try
                {
                    var slug = GenerateSlug(cmd.Name);
                    if (await _repo.SlugExistsAsync(slug))
                        slug = slug + "-" + DateTime.UtcNow.Ticks.ToString()[^6..];

                    var cat = new CategoryEntity
                    {
                        Name = cmd.Name.Trim(),
                        Slug = slug,
                        Description = cmd.Description,
                        ImageUrl = cmd.ImageUrl,
                        DisplayOrder = cmd.DisplayOrder,
                        IsActive = cmd.IsActive
                    };

                    await _repo.AddAsync(cat);
                    return await Result<int>.SuccessAsync(cat.Id, "Tạo danh mục thành công.");
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Lỗi: {ex.Message}");
                }
            }

            public static string GenerateSlug(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) return "";
                var map = new Dictionary<string, string>
            {
                { "à|á|ả|ã|ạ|ă|ắ|ặ|ằ|ẳ|ẵ|â|ấ|ầ|ẩ|ẫ|ậ", "a" },
                { "è|é|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ",               "e" },
                { "ì|í|ỉ|ĩ|ị",                             "i" },
                { "ò|ó|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ", "o" },
                { "ù|ú|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự",               "u" },
                { "ỳ|ý|ỷ|ỹ|ỵ",                             "y" },
                { "đ",                                       "d" }
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
}
