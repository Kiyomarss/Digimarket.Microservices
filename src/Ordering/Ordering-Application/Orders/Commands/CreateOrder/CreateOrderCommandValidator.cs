using FluentValidation;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator 
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Customer).NotNull().WithMessage("Customer can't be null");
    }
}