using Moq;
using Ordering.Application.Services;
using ProductGrpc;

namespace Shared.TestFixtures;

public class ProductServiceMockBuilder
{
    private readonly Mock<IProductService> _mock = new(MockBehavior.Loose);

    public ProductServiceMockBuilder WithDefaultProducts()
    {
        var response = new ReserveProductsResponse();
        response.Products.Add(new ReservedProduct { ProductId = TestGuids.Guid1, Price = 1500 });
        response.Products.Add(new ReservedProduct { ProductId = TestGuids.Guid2, Price = 2500 });

        _mock.Setup(x => x.ReserveProductsAsync(
                                                It.IsAny<ReserveProductsRequest>(), 
                                                It.IsAny<CancellationToken>()))
             .ReturnsAsync(response);

        return this;
    }

    public ProductServiceMockBuilder WithEmptyResponse()
    {
        _mock.Setup(x => x.ReserveProductsAsync(
                                                It.IsAny<ReserveProductsRequest>(), 
                                                It.IsAny<CancellationToken>()))
             .ReturnsAsync(new ReserveProductsResponse());

        return this;
    }

    public ProductServiceMockBuilder WithException()
    {
        _mock.Setup(x => x.ReserveProductsAsync(
                                                It.IsAny<ReserveProductsRequest>(), 
                                                It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("Products not found"));

        return this;
    }

    public IProductService Build() => _mock.Object;
}