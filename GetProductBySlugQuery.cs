using AutoMapper;
using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;

namespace ElectroTech.Application.Queries.Products;

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto>;

public class GetProductBySlugQueryHandler : IRequestHandler<GetProductBySlugQuery, ProductDetailDto>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetProductBySlugQueryHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ProductDetailDto> Handle(GetProductBySlugQuery q, CancellationToken ct)
    {
        var product = await _repo.GetBySlugAsync(q.Slug, ct)
            ?? throw new NotFoundException(nameof(Product), q.Slug);

        return _mapper.Map<ProductDetailDto>(product);
    }
}