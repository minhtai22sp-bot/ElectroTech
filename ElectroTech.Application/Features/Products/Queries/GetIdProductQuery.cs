using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Products.Queries
{
    public class GetIdProductQuery : IRequest<IResult<Entities.Product>>
    {
        public int Id { get; set; }

        public class GetBranchesByIdQueryHandler : IRequestHandler<GetIdProductQuery, IResult<Entities.Product>>
        {
            private readonly IProductRepository _repository;

            public GetBranchesByIdQueryHandler(IProductRepository repository)
            {
                _repository = repository;
            }

            public async Task<IResult<Entities.Product>> Handle(GetIdProductQuery query, CancellationToken cancellationToken)
            {

                var product = await _repository.GetById(query.Id);
                if (product == null)
                {
                    return await Result<Entities.Product>.FailAsync("Không tìm thấy dữ liệu");
                }
                return await Result<Entities.Product>.SuccessAsync(product);
            }
        }
    }
}
