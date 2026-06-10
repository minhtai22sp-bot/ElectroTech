using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;

namespace ElectroTech.Application.Features.Orders.Queries
{
    public class GetOrderByIdQuery : IRequest<IResult<Order>>
    {
        public int Id { get; set; }
        public Guid? UserId { get; set; }

        public class GetOrderByIdQueryHandler
            : IRequestHandler<GetOrderByIdQuery, IResult<Order>>
        {
            private readonly IOrderRepository _repo;

            public GetOrderByIdQueryHandler(IOrderRepository repo)
            {
                _repo = repo;
            }

            public async Task<IResult<Order>> Handle(
                GetOrderByIdQuery query,
                CancellationToken cancellationToken)
            {
                var order = await _repo.GetByIdAsync(query.Id);

                if (order is null)
                    return await Result<Order>.FailAsync("Không tìm thấy đơn hàng.");

                // Chỉ check quyền khi gọi từ Customer (UserId != null)
                // Admin truyền UserId = null → bỏ qua check
                if (query.UserId.HasValue && order.UserId != query.UserId)
                    return await Result<Order>.FailAsync("Bạn không có quyền xem đơn hàng này.");

                return await Result<Order>.SuccessAsync(order);
            }
        }
    }
}