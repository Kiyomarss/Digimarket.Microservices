using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Exceptions.Application;
using BuildingBlocks.UnitOfWork;
using MediatR;
using Ordering.Application.RepositoryContracts;

namespace Ordering.Application.Orders.Commands.PayOrder;

public class PayOrderHandler : ICommandHandler<PayOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PayOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id);

        if (order is null)
            throw new NotFoundException($"Order with id '{request.Id}' not found");

        order.Pay();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
