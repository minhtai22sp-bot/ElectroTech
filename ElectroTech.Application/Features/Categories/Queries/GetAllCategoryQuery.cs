using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using CategoryEntity = Entities.Categories;

namespace ElectroTech.Application.Features.Categories.Queries;

public class GetAllCategoryQuery : IRequest<IResult<List<CategoryEntity>>>
{
    public class Handler
        : IRequestHandler<GetAllCategoryQuery, IResult<List<CategoryEntity>>>
    {
        private readonly ICategoryRepository _repo;

        public Handler(ICategoryRepository repo) => _repo = repo;

        public async Task<IResult<List<CategoryEntity>>> Handle(
            GetAllCategoryQuery query, CancellationToken ct)
        {
            var data = await _repo.GetAllAsync();
            return await Result<List<CategoryEntity>>.SuccessAsync(data);
        }
    }
}