using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Brand.Commands
{
    public class UpdateBrandCommand : IRequest<Result<int>>
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(500)]
        public string? Website { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public class Handler : IRequestHandler<UpdateBrandCommand, Result<int>>
        {
            private readonly IBrandRepository _brandRepo;

            public Handler(IBrandRepository brandRepo)
                => _brandRepo = brandRepo;

            public async Task<Result<int>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
            {
                var brand = await _brandRepo.GetByIdAsync(request.Id);
                if (brand is null)
                    return Result<int>.Fail("Không tìm thấy thương hiệu.");

                var existing = await _brandRepo.GetByNameAsync(request.Name);
                if (existing != null && existing.Id != request.Id)
                    return Result<int>.Fail("Tên thương hiệu đã tồn tại.");

                brand.Name = request.Name.Trim();
                brand.Slug = GenerateSlug(request.Name);
                brand.Description = request.Description;
                brand.LogoUrl = request.LogoUrl;
                brand.Website = request.Website;
                brand.DisplayOrder = request.DisplayOrder;
                brand.IsActive = request.IsActive;

                await _brandRepo.UpdateAsync(brand);

                return Result<int>.Success(brand.Id, "Cập nhật thương hiệu thành công.");
            }

            private static string GenerateSlug(string name)
            {
                var slug = name.Trim().ToLower();
                slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
                slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
                return slug;
            }
        }
    }
}
