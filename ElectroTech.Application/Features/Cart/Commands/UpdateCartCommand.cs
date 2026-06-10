using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElectroTech.Application.Features.Cart.Commands;

public class UpdateCartCommand : IRequest<IResult<decimal>>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public class Handler : IRequestHandler<UpdateCartCommand, IResult<decimal>>
    {
        private readonly ICartRepository _repo;
        private readonly IHttpContextAccessor _http;

        public Handler(ICartRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public async Task<IResult<decimal>> Handle(
            UpdateCartCommand cmd, CancellationToken ct)
        {
            var userId = _http.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            await _repo.UpdateQuantityAsync(userId, cmd.ProductId, cmd.Quantity);

            var total = await _repo.GetTotalAsync(userId);
            return await Result<decimal>.SuccessAsync(total, "Đã cập nhật.");
        }
    }
}