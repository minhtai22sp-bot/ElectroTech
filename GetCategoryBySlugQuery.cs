using AutoMapper;
using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Categories;

public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryTreeDto>;

public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryTreeDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetCategoryBySlugQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<CategoryTreeDto> Handle(GetCategoryBySlugQuery q, CancellationToken ct)
    {
        var category = await _uow.Repository<Category>()
            .GetQueryable()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == q.Slug, ct)
            ?? throw new NotFoundException(nameof(Category), q.Slug);

        return _mapper.Map<CategoryTreeDto>(category);
    }
}