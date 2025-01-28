using System.Numerics;

using FluentValidation.TestHelper;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class ValidationTests
{
    private readonly PostPaymentValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Empty()
    {
        var model = new PostPaymentRequest { CardNumber = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber)
              .WithErrorMessage("Card number is required.");
    }
    
    [Theory]
    [InlineData("1234567890123")] //13 digits
    [InlineData("12345678901234567890")] //20 digits
    public void Should_Have_Error_When_CardNumber_Is_Invalid_Length(string cardNumber)
    {
        var model = new PostPaymentRequest { CardNumber = cardNumber };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CardNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Should_Have_Error_When_ExpiryMonth_Is_Invalid(int expiryMonth)
    {
        var model = new PostPaymentRequest { ExpiryMonth = expiryMonth, CardNumber = "", ExpiryYear = 2025};
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryMonth);
    }

    [Fact]
    public void Should_Have_Error_When_ExpiryYear_Is_In_The_Past()
    {
        var model = new PostPaymentRequest { ExpiryYear = DateTime.Now.Year - 1, CardNumber = ""};
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ExpiryYear)
              .WithErrorMessage("Expiry year must be greater than or equal to the current year.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("INVALID")]
    public void Should_Have_Error_When_Currency_Is_Invalid(string currency)
    {
        var model = new PostPaymentRequest { Currency = currency, CardNumber = ""};
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Currency)
              .WithErrorMessage("Invalid currency. Allowed values are GBP, USD, EUR.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Should_Have_Error_When_Amount_Is_Negative_Or_Zero(int amount)
    {
        var model = new PostPaymentRequest { Amount = amount, CardNumber = ""};
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Amount must be greater than 0.");
    }

    [Theory]
    [InlineData(99)]
    [InlineData(10000)]
    public void Should_Have_Error_When_Cvv_Is_Out_Of_Range(int cvv)
    {
        var model = new PostPaymentRequest { Cvv = cvv, CardNumber = ""};
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Cvv)
              .WithErrorMessage("CVV must be between 100 and 9999.");
    }

    [Fact]
    public void Should_Not_Have_Any_Errors_When_All_Fields_Are_Valid()
    {
        var model = new PostPaymentRequest
        {
            CardNumber = "1234567812345678",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.Now.Year + 1,
            Currency = "USD",
            Amount = 100.00m,
            Cvv = 123
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}