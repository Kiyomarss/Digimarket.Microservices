using Moq;
using Ordering.Core.Services;
using ProductGrpc;

namespace Shared.TestFixtures;

public class ProductServiceMockBuilder
{
    private readonly Mock<IProductService> _mock = new(MockBehavior.Loose);

    public ProductServiceMockBuilder WithDefaultProducts()
    {
        var response = new GetProductsResponse();
        response.Products.Add(new ProductInfo { ProductId = TestGuids.Guid1, ProductName = "Test Product", Price = 1500 });
        response.Products.Add(new ProductInfo { ProductId = TestGuids.Guid2, ProductName = "Another Product", Price = 2500 });

        _mock.Setup(x => x.GetProductsByIdsAsync(
                                                 It.IsAny<IEnumerable<string>>(), 
                                                 It.IsAny<CancellationToken>()))
             .ReturnsAsync(response);

        return this;
    }

    public ProductServiceMockBuilder WithEmptyResponse()
    {
        _mock.Setup(x => x.GetProductsByIdsAsync(
                                                 It.IsAny<IEnumerable<string>>(), 
                                                 It.IsAny<CancellationToken>()))
             .ReturnsAsync(new GetProductsResponse());

        return this;
    }

    public ProductServiceMockBuilder WithException()
    {
        _mock.Setup(x => x.GetProductsByIdsAsync(
                                                 It.IsAny<IEnumerable<string>>(), 
                                                 It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("Products not found"));

        return this;
    }

    public IProductService Build() => _mock.Object;
}