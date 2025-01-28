using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public class PostPaymentValidator : AbstractValidator<PostPaymentRequest>
{
    public PostPaymentValidator()
    {
        RuleFor(p => p.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Must(cardNumber => long.TryParse(cardNumber, out _)).WithMessage("Card number must be numeric.")
            .Must(cardNumber => cardNumber.Length is >= 14 and <= 19)
            .WithMessage("Card number must be between 14 and 19 digits.");

        RuleFor(p => p.ExpiryMonth)
            .NotEmpty().WithMessage("Expiry month is required.")
            .GreaterThan(0).WithMessage("Expiry month must be greater than 0.")
            .LessThan(13).WithMessage("Expiry month must be less than 13.");

        RuleFor(p => p.ExpiryYear)
            .NotEmpty().WithMessage("Expiry year is required.")
            .GreaterThanOrEqualTo(DateTime.Now.Year).WithMessage("Expiry year must be greater than or equal to the current year.")
            .Must((model, expiryYear) => IsFutureDate(expiryYear, model.ExpiryMonth))
            .WithMessage("Expiry date must be a future date.");

        RuleFor(p => p.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(BeValidCurrency).WithMessage("Invalid currency. Allowed values are GBP, USD, EUR.");

        RuleFor(p => p.Amount)
            .NotEmpty().WithMessage("Amount is required.")
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(p => p.Cvv)
            .InclusiveBetween(100, 9999).WithMessage("CVV must be between 100 and 9999.");
    }

    private bool BeValidCurrency(string currency)
    {
        string[] validCurrencies = ["GBP", "USD", "EUR"];
        return validCurrencies.Contains(currency);
    }
    
    private bool IsFutureDate(int expiryYear, int expiryMonth)
    {
        if (expiryYear == 0 || expiryMonth == 0)
        {
            return false;
        }
        
        if ((expiryYear < DateTime.Now.Year && expiryMonth < DateTime.Now.Month) || expiryMonth > 12)
            return false;
        
        var expiryDate = new DateTime(expiryYear, expiryMonth, 1);
        if (expiryDate < DateTime.Now) return false;
        return true;
    }
}