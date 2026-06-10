using AutoMapper;
using AutoMapper.QueryableExtensions;
using ElectroTech.Domain.Enums;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Orders;

public record GetOrdersQuery(
    string? Status,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderSummaryDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderRepository _repo;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IOrderRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery q, CancellationToken ct)
    {
        var query = _repo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(q.Status) && Enum.TryParse<OrderStatus>(q.Status, out var status))
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(o => o.FullName.Contains(q.Search) || o.Phone.Contains(q.Search));

        query = query.OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ProjectTo<OrderSummaryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<OrderSummaryDto>(items, total, q.Page, q.PageSize);
    }
}