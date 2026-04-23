using FluentValidation.TestHelper;
using OrderFlow.CQRS.Application.Features.Orders.Commands;
using OrderFlow.CQRS.Application.Tests.TestData;

namespace OrderFlow.CQRS.Application.Tests.Features.Orders.Commands;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _sut = new();

    [Fact]
    public void Validate_WithCompleteCommand_PassesAllRules()
    {
        var command = CreateOrderCommandFaker.Valid();

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WhenCustomerEmailIsInvalid_ProducesError(string email)
    {
        var command = CreateOrderCommandFaker.Valid() with { CustomerEmail = email };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CustomerEmail);
    }

    [Fact]
    public void Validate_WhenItemsEmpty_ProducesError()
    {
        var command = CreateOrderCommandFaker.Valid() with { Items = new List<CreateOrderItemDto>() };

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact]
    public void Validate_WhenItemQuantityIsZero_ProducesErrorOnQuantity()
    {
        var command = CreateOrderCommandFaker.Valid(CreateOrderCommandFaker.Item(quantity: 0));

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Validate_WhenUnitPriceIsZero_ProducesErrorOnUnitPrice()
    {
        var command = CreateOrderCommandFaker.Valid(CreateOrderCommandFaker.Item(unitPrice: 0m));

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}
