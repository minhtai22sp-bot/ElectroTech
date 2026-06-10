using AutoMapper;
using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Interfaces;
using MediatR;

namespace ElectroTech.Application.Commands.Products;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId,
    bool IsFeatured,
    List<string> ImageUrls,
    List<ProductSpecDto> Specs
) : IRequest<int>;

public record ProductSpecDto(string Key, string Value);

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IUnitOfWork _uow;
    private readonly ISlugService _slugService;

    public CreateProductCommandHandler(IUnitOfWork uow, ISlugService slugService)
    {
        _uow = uow;
        _slugService = slugService;
    }

    public async Task<int> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = new Product
        {
            Name = cmd.Name,
            Slug = await _slugService.GenerateAsync(cmd.Name, ct),
            Description = cmd.Description,
            Price = cmd.Price,
            Stock = cmd.Stock,
            CategoryId = cmd.CategoryId,
            IsFeatured = cmd.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            Images = cmd.ImageUrls.Select((url, i) => new ProductImage
            {
                Url = url,
                IsPrimary = i == 0
            }).ToList(),
            Specs = cmd.Specs.Select(s => new ProductSpec
            {
                Key = s.Key,
                Value = s.Value
            }).ToList()
        };

        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);
        return product.Id;
    }
}