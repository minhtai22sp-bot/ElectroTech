using AspNetCoreHero.Results;
using Entities.ViewModel;
using Interfaces;
using Library;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Products.Queries
{
   
    public class GetAllPaginatedListQuery : IRequest<IResult<PaginatedList<ProductIndexModel>>>
    {
        public ProductSearch model { get; set; }

        public class GetAllPaginatedListHandler : IRequestHandler<GetAllPaginatedListQuery, IResult<PaginatedList<ProductIndexModel>>>
        {
            private readonly IProductRepository _repository;

            public GetAllPaginatedListHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<IResult<PaginatedList<ProductIndexModel>>> Handle(
     GetAllPaginatedListQuery query, CancellationToken cancellationToken)
            {
                // ✅ guard null
                query.model ??= new ProductSearch
                {
                    pageSize = 12,
                    currentPage = 1,
                    skip = 0
                };

                var product = await _repository.GetAllPaginatedAsync(query.model);
                if (product == null)
                    return await Result<PaginatedList<ProductIndexModel>>
                        .FailAsync("Không tìm thấy dữ liệu");

                return await Result<PaginatedList<ProductIndexModel>>
                    .SuccessAsync(product);
            }
        }
    }
}
