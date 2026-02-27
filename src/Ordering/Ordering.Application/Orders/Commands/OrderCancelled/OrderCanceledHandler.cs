using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions.Application;
using BuildingBlocks.UnitOfWork;
using MediatR;
using Ordering.Application.RepositoryContracts;

namespace Ordering.Application.Orders.Commands.OrderCancelled;

public class OrderCanceledHandler : ICommandHandler<OrderCanceledCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderCanceledHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(OrderCanceledCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id);

        if (order is null)
            throw new NotFoundException($"Order with id '{request.Id}' not found");

        order.Canceled();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
