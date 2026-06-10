using AspNetCoreHero.Results;
using Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElectroTech.Application.Features.Cart.Commands;

public class ClearCartCommand : IRequest<IResult<bool>>
{
    public class Handler : IRequestHandler<ClearCartCommand, IResult<bool>>
    {
        private readonly ICartRepository _repo;
        private readonly IHttpContextAccessor _http;

        public Handler(ICartRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public async Task<IResult<bool>> Handle(
            ClearCartCommand cmd, CancellationToken ct)
        {
            var userId = _http.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            await _repo.ClearAsync(userId);
            return await Result<bool>.SuccessAsync(true, "Đã xóa giỏ hàng.");
        }
    }
}