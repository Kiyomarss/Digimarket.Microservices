using Basket_Application.RepositoryContracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.UnitOfWork;
using MediatR;

namespace Basket_Application.Basket.Commands.RemoveBasket;

public class RemoveBasketHandler : ICommandHandler<RemoveBasketCommand>
{
    private readonly IBasketRepository _basketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBasketHandler(IUnitOfWork unitOfWork, IBasketRepository basketRepository)
    {
        _unitOfWork = unitOfWork;
        _basketRepository = basketRepository;
    }

    public async Task<Unit> Handle(RemoveBasketCommand request, CancellationToken cancellationToken)
    {
        await _basketRepository.DeleteBasketItemByUserId(request.UserId);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
