using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Application.Features.Orders.Queries
{
    public class GetUserOrdersQuery : IRequest<IResult<List<Order>>>
    {
        public string UserId { get; set; }

        public class GetUserOrdersQueryHandler
            : IRequestHandler<GetUserOrdersQuery, IResult<List<Order>>>
        {
            private readonly IOrderRepository _repo;

            public GetUserOrdersQueryHandler(IOrderRepository repo)
            {
                _repo = repo;
            }

            public async Task<IResult<List<Order>>> Handle(
                GetUserOrdersQuery query, CancellationToken cancellationToken)
            {
                var orders = await _repo.GetByUserIdAsync(query.UserId);
                return await Result<List<Order>>.SuccessAsync(orders);
            }
        }
    }
}
