using AspNetCoreHero.Results;
using Entities;
using Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ElectroTech.Application.Features.Cart.Commands;

public class AddToCartCommand : IRequest<IResult<int>>
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = "";
    public int Quantity { get; set; } = 1;

    public class Handler : IRequestHandler<AddToCartCommand, IResult<int>>
    {
        private readonly ICartRepository _repo;
        private readonly IHttpContextAccessor _http;

        public Handler(ICartRepository repo, IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        public async Task<IResult<int>> Handle(
            AddToCartCommand cmd, CancellationToken ct)
        {
            var userId = _http.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            await _repo.AddItemAsync(userId, new CartItem
            {
                ProductId = cmd.ProductId,
                ProductName = cmd.ProductName,
                Price = cmd.Price,
                ImageUrl = cmd.ImageUrl,
                Quantity = cmd.Quantity
            });

            var count = await _repo.GetCountAsync(userId);
            return await Result<int>.SuccessAsync(count, "Đã thêm vào giỏ hàng.");
        }
    }
}