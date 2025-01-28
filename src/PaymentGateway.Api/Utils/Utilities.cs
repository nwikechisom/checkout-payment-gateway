namespace PaymentGateway.Api.Utils;

public static class Utilities
{
    public static int ToMinorCurrencyUnit(this decimal amount)
    {
        return (int)(amount * 100);
    }

    public static  decimal ToMajorCurrencyUnit(this int minorAmount)
    {
        return minorAmount / 100m;
    }

    public static int GetLastFourDigits(this string cardNumber)
    {
        var lastDigits = cardNumber.Substring(cardNumber.Length-4);
        return int.Parse(lastDigits);
    }

    public static string GenerateExpiryDate(int expiryYear, int expiryMonth)
    {
        return $"{expiryMonth:D2}/{expiryYear}";
    }
}