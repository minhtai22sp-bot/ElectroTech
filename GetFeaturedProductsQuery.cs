using AutoMapper;
using AutoMapper.QueryableExtensions;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Products;

public record GetFeaturedProductsQuery(int Take = 8) : IRequest<List<ProductDto>>;

public class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetFeaturedProductsQueryHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(GetFeaturedProductsQuery q, CancellationToken ct)
        => await _repo.GetQueryable()
            .Where(p => p.IsFeatured && p.Stock > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Take(q.Take)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
}