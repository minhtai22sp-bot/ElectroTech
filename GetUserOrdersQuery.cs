using AutoMapper;
using AutoMapper.QueryableExtensions;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Orders;

public record GetUserOrdersQuery(
    string UserId,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<OrderSummaryDto>>;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderRepository _repo;
    private readonly IMapper _mapper;

    public GetUserOrdersQueryHandler(IOrderRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetUserOrdersQuery q, CancellationToken ct)
    {
        var query = _repo.GetQueryable()
            .Where(o => o.UserId == q.UserId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ProjectTo<OrderSummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<OrderSummaryDto>(items, total, q.Page, q.PageSize);
    }
}