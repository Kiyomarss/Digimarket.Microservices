using BuildingBlocks.CQRS;
using BuildingBlocks.UnitOfWork;
using Catalog.Application.RepositoryContracts;

namespace Catalog.Application.Products.CreateOrder;

public class ReservedProductCommandHandler : ICommandHandler<ReserveProductsCommand, ReserveProductsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;

    public ReservedProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReserveProductsResponse> Handle(ReserveProductsCommand request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetProductByIds(request.Items.Select(x => x.ProductId).ToList(), cancellationToken);

        foreach (var item in products)
        {
            var quantity = request.Items.Single(p => p.ProductId == item.Id).Quantity;

            item.DecreaseStock(quantity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReserveProductsResponse(products.Select(p => new ReservedProductCommand(p.Id, p.Price)));
    }
}