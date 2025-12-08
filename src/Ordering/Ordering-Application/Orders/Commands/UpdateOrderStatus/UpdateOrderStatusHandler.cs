
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering_Domain.Domain.RepositoryContracts;
using Ordering.Application.Orders.Commands.CreateOrder;

namespace Ordering.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler : ICommandHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusHandler(IUnitOfWork unitOfWork, ILogger<UpdateOrderStatusHandler> logger, IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id);

        if (order is null)
            throw new NotFoundException($"Order with id '{request.Id}' not found");

        order.State = request.State;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
