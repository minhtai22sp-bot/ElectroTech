using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Categories.commands
{
    public class UpdateCategoryCommand : IRequest<IResult<int>>
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public class Handler : IRequestHandler<UpdateCategoryCommand, IResult<int>>
        {
            private readonly ICategoryRepository _repo;
            public Handler(ICategoryRepository repo) => _repo = repo;

            public async Task<IResult<int>> Handle(
                UpdateCategoryCommand cmd, CancellationToken ct)
            {
                try
                {
                    var cat = await _repo.GetByIdAsync(cmd.Id);
                    if (cat is null)
                        return await Result<int>.FailAsync("Không tìm thấy danh mục.");

                    cat.Name = cmd.Name.Trim();
                    cat.Slug = CreateCategoryCommand.Handler.GenerateSlug(cmd.Name);
                    cat.Description = cmd.Description;
                    cat.ImageUrl = cmd.ImageUrl;
                    cat.DisplayOrder = cmd.DisplayOrder;
                    cat.IsActive = cmd.IsActive;

                    await _repo.UpdateAsync(cat);
                    return await Result<int>.SuccessAsync(cat.Id, "Cập nhật danh mục thành công.");
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Lỗi: {ex.Message}");
                }
            }
        }
    }
}
