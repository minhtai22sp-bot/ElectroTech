using AspNetCoreHero.Results;
using Entities;          
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Brands.Commands   
{
    public class CreateBrandCommand : IRequest<Result<int>>
    {
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
        public bool IsActive { get; set; } = true;

        public class Handler : IRequestHandler<CreateBrandCommand, Result<int>>
        {
            private readonly IBrandRepository _brandRepo;
            public Handler(IBrandRepository brandRepo)
                => _brandRepo = brandRepo;

            public async Task<Result<int>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
            {
                var existing = await _brandRepo.GetByNameAsync(request.Name);
                if (existing != null)
                    return Result<int>.Fail("Tên thương hiệu đã tồn tại.");

                var brand = new Entities.Brand
                {
                    Name = request.Name.Trim(),
                    Slug = GenerateSlug(request.Name),
                    Description = request.Description,
                    LogoUrl = request.LogoUrl,
                    Website = request.Website,
                    DisplayOrder = request.DisplayOrder,
                    IsActive = request.IsActive
                };

                var created = await _brandRepo.AddAsync(brand);
                return Result<int>.Success(created.Id, "Tạo thương hiệu thành công.");
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