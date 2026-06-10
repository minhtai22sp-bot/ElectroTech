using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;

namespace ElectroTech.Application.Commands.Products;

public record UpdateProductCommand(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    int CategoryId,
    bool IsFeatured,
    List<string> ImageUrls,
    List<ProductSpecDto> Specs
) : IRequest<bool>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IUnitOfWork _uow;
    private readonly ISlugService _slugService;

    public UpdateProductCommandHandler(IUnitOfWork uow, ISlugService slugService)
    {
        _uow = uow;
        _slugService = slugService;
    }

    public async Task<bool> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        product.Name = cmd.Name;
        product.Slug = await _slugService.GenerateAsync(cmd.Name, ct);
        product.Description = cmd.Description;
        product.Price = cmd.Price;
        product.Stock = cmd.Stock;
        product.CategoryId = cmd.CategoryId;
        product.IsFeatured = cmd.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        product.Images.Clear();
        product.Images = cmd.ImageUrls.Select((url, i) => new ProductImage
        {
            Url = url,
            IsPrimary = i == 0,
            ProductId = product.Id
        }).ToList();

        product.Specs.Clear();
        product.Specs = cmd.Specs.Select(s => new ProductSpec
        {
            Key = s.Key,
            Value = s.Value,
            ProductId = product.Id
        }).ToList();

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}