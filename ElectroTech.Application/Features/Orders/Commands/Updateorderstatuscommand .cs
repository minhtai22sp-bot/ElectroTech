using AspNetCoreHero.Results;
using Enums;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Orders.Commands
{
    public class UpdateOrderStatusCommand : IRequest<IResult>
    {
        public int Id { get; set; }
        public string Status { get; set; } = "";

        public class UpdateOrderStatusCommandHandler
            : IRequestHandler<UpdateOrderStatusCommand, IResult>
        {
            private readonly IOrderRepository _repo;

            public UpdateOrderStatusCommandHandler(IOrderRepository repo)
            {
                _repo = repo;
            }

            public async Task<IResult> Handle(
                UpdateOrderStatusCommand command, CancellationToken cancellationToken)
            {
                var order = await _repo.GetByIdAsync(command.Id);
                if (order is null)
                    return await Result.FailAsync("Không tìm thấy đơn hàng.");

                if (!Enum.TryParse<OrderStatus>(command.Status, out var status))
                    return await Result.FailAsync($"Trạng thái '{command.Status}' không hợp lệ.");

                order.Status = status;
                await _repo.UpdateAsync(order);

                return await Result.SuccessAsync("Cập nhật trạng thái thành công.");
            }
        }
    }

}
