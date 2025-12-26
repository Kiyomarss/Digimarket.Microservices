using BuildingBlocks.CQRS;

namespace Catalog.Application.Products.Queries;

public record GetProductsByIdsQuery(List<Guid> ProductIds) : IQuery<GetProductsByIdsResponse>;

public record GetProductsByIdsResponse(List<ProductDto> Products);

public record ProductDto(Guid Id, string Name, long Price);