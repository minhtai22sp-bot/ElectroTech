// Application/Features/Products/Queries/GetSidebarStatsQuery.cs
using AspNetCoreHero.Results;
using Entities.ViewModel;
using Interfaces;
using MediatR;

namespace ElectroTech.Application.Features.Products.Queries;

public class GetSidebarStatsQuery : IRequest<IResult<ProductSidebarStats>>
{
    public string? Keyword { get; set; }

    public class GetSidebarStatsHandler
        : IRequestHandler<GetSidebarStatsQuery, IResult<ProductSidebarStats>>
    {
        private readonly IProductRepository _repository;

        public GetSidebarStatsHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IResult<ProductSidebarStats>> Handle(
            GetSidebarStatsQuery query, CancellationToken cancellationToken)
        {
            var stats = await _repository.GetSidebarStatsAsync(query.Keyword);
            return await Result<ProductSidebarStats>.SuccessAsync(stats);
        }
    }
}