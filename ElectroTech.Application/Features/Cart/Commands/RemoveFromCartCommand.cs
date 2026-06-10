using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElectroTech.Application.Features.Cart.Commands;

public class RemoveFromCartCommand : IRequest<IResult<int>>
{
    public int ProductId { get; set; }

    public class Handler : IRequestHandler<RemoveFromCartCommand, IResult<int>>
    {
        private readonly ICartRepository _repo;
        private readonly IHttpContextAccessor _http;

        public Handler(ICartRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public async Task<IResult<int>> Handle(
            RemoveFromCartCommand cmd, CancellationToken ct)
        {
            var userId = _http.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            await _repo.RemoveItemAsync(userId, cmd.ProductId);

            var count = await _repo.GetCountAsync(userId);
            return await Result<int>.SuccessAsync(count, "Đã xóa.");
        }
    }
}