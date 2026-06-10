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
    public class UpdatePaymentStatusCommand : IRequest<IResult>
    {
        public int Id { get; set; }
        public string PaymentStatus { get; set; } = "";

        public class UpdatePaymentStatusCommandHandler
            : IRequestHandler<UpdatePaymentStatusCommand, IResult>
        {
            private readonly IOrderRepository _repo;

            public UpdatePaymentStatusCommandHandler(IOrderRepository repo)
            {
                _repo = repo;
            }

            public async Task<IResult> Handle(
                UpdatePaymentStatusCommand command, CancellationToken cancellationToken)
            {
                var order = await _repo.GetByIdAsync(command.Id);
                if (order is null)
                    return await Result.FailAsync("Không tìm thấy đơn hàng.");

                if (!Enum.TryParse<PaymentStatus>(command.PaymentStatus, out var paymentStatus))
                    return await Result.FailAsync($"Trạng thái '{command.PaymentStatus}' không hợp lệ.");

                order.PaymentStatus = paymentStatus;
                await _repo.UpdateAsync(order);

                return await Result.SuccessAsync("Cập nhật thanh toán thành công.");
            }
        }
    }
}
