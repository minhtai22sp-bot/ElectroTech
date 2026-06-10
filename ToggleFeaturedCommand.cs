using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;

namespace ElectroTech.Application.Commands.Products;

public record ToggleFeaturedCommand(int Id) : IRequest<bool>;

public class ToggleFeaturedCommandHandler : IRequestHandler<ToggleFeaturedCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public ToggleFeaturedCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(ToggleFeaturedCommand cmd, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        product.IsFeatured = !product.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);
        return product.IsFeatured;
    }
}