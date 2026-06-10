using AutoMapper;
using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ElectroTech.Application.Queries.Categories;

public record GetCategoriesTreeQuery : IRequest<List<CategoryTreeDto>>;

public class GetCategoriesTreeQueryHandler : IRequestHandler<GetCategoriesTreeQuery, List<CategoryTreeDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetCategoriesTreeQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<CategoryTreeDto>> Handle(GetCategoriesTreeQuery _, CancellationToken ct)
    {
        var all = await _uow.Repository<Category>()
            .GetQueryable()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return BuildTree(all, parentId: null);
    }

    private List<CategoryTreeDto> BuildTree(List<Category> all, int? parentId)
        => all
            .Where(c => c.ParentId == parentId)
            .Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                Children = BuildTree(all, c.Id)
            })
            .ToList();
}