using FluentValidation;
using Ordering.Application.Orders.Validation;

namespace Ordering.Application.Orders.Queries;

public class GetCurrentUserOrdersQueryValidator : AbstractValidator<GetCurrentUserOrdersQuery>
{
    public GetCurrentUserOrdersQueryValidator()
    {
        RuleFor(x => x.State).SetValidator(new OrderStateValidator());
    }
}