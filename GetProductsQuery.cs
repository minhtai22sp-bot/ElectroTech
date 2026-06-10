using AutoMapper.QueryableExtensions;
using AutoMapper;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Products;

public record GetProductsQuery(
    string? Search,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? IsFeatured,
    string SortBy = "createdAt",
    bool Descending = true,
    int Page = 1,
    int PageSize = 12
) : IRequest<PagedResult<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery q, CancellationToken ct)
    {
        var query = _repo.GetQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => p.Name.Contains(q.Search));

        if (q.CategoryId.HasValue) query = query.Where(p => p.CategoryId == q.CategoryId.Value);
        if (q.MinPrice.HasValue) query = query.Where(p => p.Price >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue) query = query.Where(p => p.Price <= q.MaxPrice.Value);
        if (q.IsFeatured.HasValue) query = query.Where(p => p.IsFeatured == q.IsFeatured.Value);

        query = q.SortBy.ToLower() switch
        {
            "price" => q.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "name" => q.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            _ => q.Descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, total, q.Page, q.PageSize);
    }
}