using FluentValidation;

namespace Ordering.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        // 2. Items list
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Order items cannot be null.")
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= 50).WithMessage("Order cannot contain more than 50 items."); // محدودیت منطقی

        // 3. هر Item در لیست
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required for each item.")
                .MaximumLength(50).WithMessage("ProductId cannot exceed 50 characters.")
                .Must(BeAValidGuid).WithMessage("ProductId must be a valid GUID.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 per item."); // محدودیت انبار یا منطقی
        });

        // 4. سناریوی پیشرفته: جلوگیری از تکرار ProductId در آیتم‌ها
        RuleFor(x => x.Items)
            .Must(HaveUniqueProductIds)
            .WithMessage("Duplicate products are not allowed in the order.");
    }

    //TODO: متد زیر می‌تواند به تنهای استفاده شود و شروط دیگری در کنار آن به صورت اضافه ست نشود
    // متد کمکی برای چک GUID
    private static bool BeAValidGuid(string productId)
    {
        return Guid.TryParse(productId, out _);
    }

    // متد کمکی برای چک تکراری نبودن ProductId
    private static bool HaveUniqueProductIds(List<CreateOrderCommand.OrderItemDto> items)
    {
        if (items.Count == 0) return true;
        var productIds = items.Select(i => i.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }
}