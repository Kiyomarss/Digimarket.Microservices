using Catalog.Application.Products.Queries;
using Grpc.Core;
using MediatR;
using ProductGrpc;

namespace Catalog.Api.Grpc;

public class ProductGrpcService : ProductProtoService.ProductProtoServiceBase
{
    private readonly ISender _sender;

    public ProductGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<GetProductsResponse> GetProductsByIds(GetProductsRequest request, ServerCallContext context)
    {
        var productIds = request.ProductIds
                                .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                                .Where(g => g != Guid.Empty)
                                .ToList();

        var query = new GetProductsByIdsQuery(productIds);
        var result = await _sender.Send(query);

        var response = new GetProductsResponse();
        foreach (var p in result.Products)
        {
            response.Products.Add(new ProductInfo
            {
                ProductId = p.Id.ToString(),
                ProductName = p.Name,
                Price = p.Price
            });
        }

        return response;
    }
}