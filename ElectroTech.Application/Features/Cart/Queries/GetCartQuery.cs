using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElectroTech.Application.Features.Cart.Queries;

public class GetCartQuery : IRequest<IResult<List<CartItem>>>
{
    public class Handler : IRequestHandler<GetCartQuery, IResult<List<CartItem>>>
    {
        private readonly ICartRepository _repo;
        private readonly IHttpContextAccessor _http;

        public Handler(ICartRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public async Task<IResult<List<CartItem>>> Handle(
            GetCartQuery query, CancellationToken ct)
        {
            var userId = _http.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var items = await _repo.GetItemsAsync(userId);
            return await Result<List<CartItem>>.SuccessAsync(items);
        }
    }
}