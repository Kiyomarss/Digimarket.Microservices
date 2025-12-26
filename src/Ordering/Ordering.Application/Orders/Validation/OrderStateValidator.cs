using FluentValidation;
using Ordering_Domain.Domain.Enum;

namespace Ordering.Application.Orders.Validation;

public class OrderStateValidator : AbstractValidator<string>
{
    private readonly string[] _validStates = OrderState.All.Select(s => s.Code).ToArray();

    public OrderStateValidator()
    {
        RuleFor(state => state)
            .Cascade(CascadeMode.Stop) // مهم: وقتی خطا داد، قوانین بعدی اجرا نشوند
            .NotEmpty()
            .WithMessage("وضعیت سفارش الزامی است.")
            .Must(BeAValidStateCode)
            .WithMessage("وضعیت سفارش نامعتبر است.");
    }

    private bool BeAValidStateCode(string? state)
    {
        return _validStates.Contains(state, StringComparer.OrdinalIgnoreCase);
    }
}