using Basket_Application.RepositoryContracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Services;
using BuildingBlocks.UnitOfWork;
using OrderGrpc;

namespace Basket_Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IBasketRepository _basketRepository;
    private readonly OrderProtoService.OrderProtoServiceClient _orderProto;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(
        IBasketRepository basketRepository,
        OrderProtoService.OrderProtoServiceClient orderProto,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _basketRepository = basketRepository;
        _orderProto = orderProto;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var userId = await _currentUser.GetRequiredUserId();
        
        var basket = await _basketRepository.FindBasketByUserId(userId);

        if (basket == null)
            throw new Exception("Basket not found.");

        var request = new CreateOrderRequest
        {
            Customer = _currentUser.GetUserName() ?? "unknown"
        };

        request.Items.AddRange(basket.Items.Select(x => new OrderItemDto
        {
            ProductId = x.ProductId.ToString(), Quantity = x.Quantity
        }));

        var headers = _currentUser.GetAuthorizationHeaders();
        var response = await _orderProto.CreateOrderAsync(request, headers, cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(response.OrderId);
    }
}