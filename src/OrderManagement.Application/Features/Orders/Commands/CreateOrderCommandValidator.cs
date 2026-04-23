using FluentValidation;

namespace OrderFlow.CQRS.Application.Features.Orders.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Müşteri adı zorunludur.")
            .MaximumLength(200).WithMessage("Müşteri adı 200 karakterden uzun olamaz.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Müşteri e-postası zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Teslimat adresi zorunludur.")
            .MaximumLength(500).WithMessage("Teslimat adresi 500 karakterden uzun olamaz.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("En az bir ürün eklenmelidir.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Ürün adı zorunludur.");

            item.RuleFor(x => x.ProductCode)
                .NotEmpty().WithMessage("Ürün kodu zorunludur.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Birim fiyat 0'dan büyük olmalıdır.");
        });
    }
}
