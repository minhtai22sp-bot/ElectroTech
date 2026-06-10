using AutoMapper;
using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Orders;

public record GetOrderByIdQuery(int Id, string? UserId = null) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery q, CancellationToken ct)
    {
        var order = await _repo.GetQueryable()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == q.Id, ct)
            ?? throw new NotFoundException(nameof(Order), q.Id);

        if (q.UserId is not null && order.UserId != q.UserId)
            throw new BusinessRuleException("Bạn không có quyền xem đơn hàng này.");

        return _mapper.Map<OrderDto>(order);
    }
}