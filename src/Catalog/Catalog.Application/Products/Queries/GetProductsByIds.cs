using BuildingBlocks.CQRS;

namespace Catalog.Application.Products.Queries;

public record GetProductsByIdsQuery(IEnumerable<Guid> ProductIds) : IQuery<GetProductsByIdsResponse>;

public record GetProductsByIdsResponse(IEnumerable<ProductDto> Products);

public record ProductDto(Guid Id, long Price);