using BuildingBlocks.Common.Extensions;
using Catalog.Application.Products.CreateOrder;
using Catalog.Application.Products.Queries;
using Grpc.Core;
using MediatR;
using ProductGrpc;
using ReserveProductsResponse = ProductGrpc.ReserveProductsResponse;

namespace Catalog.Api.Grpc;

//TODO شاید بهتر باشد برای این کلاس نیز مانند قسمت clint آن از اینترفیس استفاده شود
public class ProductGrpcService : ProductProtoService.ProductProtoServiceBase
{
    private readonly ISender _sender;

    public ProductGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<ReserveProductsResponse> ReserveProducts(
        ReserveProductsRequest request,
        ServerCallContext context)
    {
        var items = request.Items.Select(x => new OrderItemDto(Guid.Parse(x.ProductId), x.Quantity)).ToList();
        var command = new ReserveProductsCommand(items);
        var result = await _sender.Send(command);

        var response = new ReserveProductsResponse();
        response.Products.AddRange(
                                   result.Products.Select(p => new ReservedProduct
                                   {
                                       ProductId = p.ProductId.ToString(), Price = p.Price
                                   })
                                  );

        return response;
    }
    
}