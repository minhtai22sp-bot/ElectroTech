using ElectroTech.Domain.Entities;
using ElectroTech.Domain.Exceptions;
using ElectroTech.Domain.Interfaces;
using MediatR;

namespace ElectroTech.Application.Commands.Products;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        _uow.Products.Delete(product);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}