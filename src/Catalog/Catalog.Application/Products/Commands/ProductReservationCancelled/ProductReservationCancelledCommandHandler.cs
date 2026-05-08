using BuildingBlocks.CQRS;
using BuildingBlocks.UnitOfWork;
using Catalog.Application.RepositoryContracts;
using MediatR;

namespace Catalog.Application.Products.Commands.ProductReservationCancelled;

public class ProductReservationCancelledCommandHandler : ICommandHandler<ProductReservationCancelledCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;

    public ProductReservationCancelledCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ProductReservationCancelledCommand request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetProductByIds(request.Items.Select(x => x.ProductId).ToList(), cancellationToken);

        foreach (var item in products)
        {
            var quantity = request.Items.Single(p => p.ProductId == item.Id).Quantity;

            item.IncreaseStock(quantity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}