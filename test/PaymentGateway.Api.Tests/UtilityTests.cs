using PaymentGateway.Api.Utils;

namespace PaymentGateway.Api.Tests;
public class UtilityTests
{
    [Theory]
    [InlineData(100.75, 10075)]
    [InlineData(0.01, 1)]
    [InlineData(123.456, 12345)]
    [InlineData(123.999, 12399)]
    [InlineData(0, 0)]
    [InlineData(-100.75, -10075)]
    public void ToMinorCurrencyUnit_Should_Return_Correct_Result(decimal amount, int expectedMinorUnits)
    {
        // Act
        var result = amount.ToMinorCurrencyUnit();

        // Assert
        Assert.Equal(expectedMinorUnits, result);
    }

    [Theory]
    [InlineData(10075, 100.75)]
    [InlineData(1, 0.01)]
    [InlineData(12399, 123.99)]
    [InlineData(12345, 123.45)]
    [InlineData(0, 0)]
    [InlineData(-10075, -100.75)]
    public void ToMajorCurrencyUnit_Should_Return_Correct_Result(int minorAmount, decimal expectedMajorUnits)
    {
        // Act
        var result = minorAmount.ToMajorCurrencyUnit();

        // Assert
        Assert.Equal(expectedMajorUnits, result);
    }

    [Theory]
    [InlineData("1234567812345678", 5678)]
    [InlineData("4000000000000000", 0000)]
    [InlineData("1111111111111111", 1111)]
    [InlineData("9876543219876543", 6543)] 
    public void GetLastFourDigits_Should_Return_Correct_Result(string cardNumber, int expectedLastFourDigits)
    {
        // Act
        var result = cardNumber.GetLastFourDigits();

        // Assert
        Assert.Equal(expectedLastFourDigits, result);
    }

    [Theory]
    [InlineData(2025, 1, "01/2025")]
    [InlineData(2025, 12, "12/2025")]
    [InlineData(2025, 6, "06/2025")]
    [InlineData(2023, 0, "00/2023")]
    [InlineData(2023, 13, "13/2023")]
    [InlineData(0, 5, "05/0")]
    public void GenerateExpiryDate_Should_Return_Correct_Result(int expiryYear, int expiryMonth, string expectedExpiryDate)
    {
        // Act
        var result = Utilities.GenerateExpiryDate(expiryYear, expiryMonth);

        // Assert
        Assert.Equal(expectedExpiryDate, result);
    }
}
