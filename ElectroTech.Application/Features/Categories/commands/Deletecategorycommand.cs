using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Categories.commands
{
    public class DeleteCategoryCommand : IRequest<IResult<int>>
    {
        public int Id { get; set; }

        public class Handler : IRequestHandler<DeleteCategoryCommand, IResult<int>>
        {
            private readonly ICategoryRepository _repo;
            public Handler(ICategoryRepository repo) => _repo = repo;

            public async Task<IResult<int>> Handle(
                DeleteCategoryCommand cmd, CancellationToken ct)
            {
                try
                {
                    var cat = await _repo.GetByIdAsync(cmd.Id);
                    if (cat is null)
                        return await Result<int>.FailAsync("Không tìm thấy danh mục.");

                    await _repo.DeleteAsync(cat);
                    return await Result<int>.SuccessAsync(cmd.Id, "Đã xóa danh mục.");
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Lỗi: {ex.Message}");
                }
            }
        }
    }

}
