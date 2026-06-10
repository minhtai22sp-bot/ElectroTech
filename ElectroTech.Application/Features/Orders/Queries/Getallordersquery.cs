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
    public class GetAllOrdersQuery : IRequest<IResult<List<Order>>>
    {
        public string? Status { get; set; }

        public class GetAllOrdersQueryHandler
            : IRequestHandler<GetAllOrdersQuery, IResult<List<Order>>>
        {
            private readonly IOrderRepository _repo;

            public GetAllOrdersQueryHandler(IOrderRepository repo)
            {
                _repo = repo;
            }

            public async Task<IResult<List<Order>>> Handle(
                GetAllOrdersQuery query, CancellationToken cancellationToken)
            {
                var orders = await _repo.GetAllAsync();

                if (!string.IsNullOrEmpty(query.Status))
                    orders = orders
                        .Where(o => o.Status.ToString() == query.Status)
                        .ToList();

                return await Result<List<Order>>.SuccessAsync(orders);
            }
        }
    }
}
