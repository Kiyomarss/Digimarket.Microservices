using Catalog.Components;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Product;
using Product.Grpc;
using ProductService = Product.Grpc.ProductService;

public class ProductServiceImpl : ProductService.ProductServiceBase
{
    private readonly ProductDbContext _db;

    public ProductServiceImpl(ProductDbContext db)
    {
        _db = db;
    }

    public override async Task<ReserveProductResponse> ReserveProduct(ReserveProductRequest request, ServerCallContext context)
    {
        var productId = Guid.Parse(request.ProductId);

        var product = await _db.Products.FirstOrDefaultAsync();

        await _db.SaveChangesAsync();

        return new ReserveProductResponse
        {
            Success = true,
            Message = "Reservation successful"
        };
    }
}