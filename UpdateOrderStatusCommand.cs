using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Enums;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;

namespace ElectroTech.Application.Commands.Orders;

public record UpdateOrderStatusCommand(
    int OrderId,
    string Status,
    string AdminId
) : IRequest<bool>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly IAdminLogService _log;

    public UpdateOrderStatusCommandHandler(IUnitOfWork uow, IAdminLogService log)
    {
        _uow = uow;
        _log = log;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand cmd, CancellationToken ct)
    {
        var order = await _uow.Orders.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), cmd.OrderId);

        if (order.Status == OrderStatus.Cancelled)
            throw new BusinessRuleException("Không thể thay đổi trạng thái đơn đã huỷ.");

        if (order.Status == OrderStatus.Delivered)
            throw new BusinessRuleException("Không thể thay đổi trạng thái đơn đã giao.");

        var oldStatus = order.Status;
        order.Status = Enum.Parse<OrderStatus>(cmd.Status);
        order.UpdatedAt = DateTime.UtcNow;

        _uow.Orders.Update(order);
        await _log.WriteAsync(cmd.AdminId,
            $"Order #{order.Id}: {oldStatus} → {order.Status}", ct);

        await _uow.SaveChangesAsync(ct);
        return true;
    }
}