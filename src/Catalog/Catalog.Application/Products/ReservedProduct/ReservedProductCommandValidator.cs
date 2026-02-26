using FluentValidation;

namespace Catalog.Application.Products.ReservedProduct;

public class ReservedProductCommandValidator : AbstractValidator<ReserveProductsCommand>
{
    public ReservedProductCommandValidator()
    {
        // 2. Items list
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= 50).WithMessage("Order cannot contain more than 50 items."); // محدودیت منطقی

        // 3. هر Item در لیست
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 per item."); // محدودیت انبار یا منطقی
        });
    }
}