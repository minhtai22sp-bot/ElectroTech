using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Brand.Commands
{
    public class DeleteBrandCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }

        public class Handler : IRequestHandler<DeleteBrandCommand, Result<int>>
        {
            private readonly IBrandRepository _brandRepo;

            public Handler(IBrandRepository brandRepo)
                => _brandRepo = brandRepo;

            public async Task<Result<int>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
            {
                var brand = await _brandRepo.GetByIdAsync(request.Id);
                if (brand is null)
                    return Result<int>.Fail("Không tìm thấy thương hiệu.");

                var hasProducts = await _brandRepo.HasProductsAsync(request.Id);
                if (hasProducts)
                    return Result<int>.Fail("Không thể xóa vì thương hiệu đang có sản phẩm liên kết.");

                await _brandRepo.DeleteAsync(brand);

                return Result<int>.Success(request.Id, "Xóa thương hiệu thành công.");
            }
        }
    }
}
