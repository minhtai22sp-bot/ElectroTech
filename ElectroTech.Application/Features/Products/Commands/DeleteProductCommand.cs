using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Products.Commands
{
    public class DeleteProductCommand : IRequest<IResult<int>>
    {
        public int Id { get; set; }

        public class DeleteProductCommandHandler
            : IRequestHandler<DeleteProductCommand, IResult<int>>
        {
            private readonly IProductRepository _repository;

            public DeleteProductCommandHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<IResult<int>> Handle(
                DeleteProductCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    var product = await _repository.GetById(command.Id);
                    if (product is null)
                        return await Result<int>.FailAsync("Không tìm thấy sản phẩm.");

                    await _repository.DeleteAsync(command.Id);
                    return await Result<int>.SuccessAsync(command.Id, "Xóa thành công.");
                }
                catch (Exception ex)
                {
                    return await Result<int>.FailAsync($"Lỗi: {ex.Message}");
                }
            }
        }
    }
}
