using BuildingBlocks.Common.Extensions;
using Catalog.Application.Products.Queries;
using Grpc.Core;
using MediatR;
using ProductGrpc;

namespace Catalog.Api.Grpc;

//TODO شاید بهتر باشد برای این کلاس نیز مانند قسمت clint آن از اینترفیس استفاده شود
public class ProductGrpcService : ProductProtoService.ProductProtoServiceBase
{
    private readonly ISender _sender;

    public ProductGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<GetProductsResponse> GetProductsByIds(GetProductsRequest request, ServerCallContext context)
    {
        var productIds = request.ProductIds.ToValidGuids().ToList();

        var query = new GetProductsByIdsQuery(productIds);
        var result = await _sender.Send(query);

        var response = new GetProductsResponse();
        foreach (var p in result.Products)
        {
            response.Products.Add(new ProductInfo
            {
                ProductId = p.Id.ToString(),
                Price = p.Price
            });
        }

        return response;
    }
}