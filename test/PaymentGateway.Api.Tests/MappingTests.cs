using AutoMapper;
using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Mapping;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests;

public class MapProfileTests
{
    private readonly IMapper _mapper;

    public MapProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MapProfile>();
        });
        _mapper = config.CreateMapper();
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Should_Map_PostPaymentRequest_To_Transaction_Correctly()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            Amount = 100.75M,
            Currency = "USD",
            MerchantId = "12345",
            CardNumber = "1234567812345678",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Cvv = 123
        };

        // Act
        var transaction = _mapper.Map<Transaction>(request);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(10075, transaction.Amount);
        Assert.Equal("USD", transaction.Currency);
        Assert.Equal("12345", transaction.Merchant);
        Assert.Equal(5678, transaction.CardNumberLastFour);
        Assert.Equal(12, transaction.ExpiryMonth);
        Assert.Equal(2025, transaction.ExpiryYear);
    }

    [Fact]
    public void Should_Map_Transaction_To_PostPaymentResponse_Correctly()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = 10075,
            Currency = "USD",
            Merchant = "12345",
            CardNumberLastFour = 5678,
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Status = PaymentStatus.Authorized
        };

        // Act
        var response = _mapper.Map<PostPaymentResponse>(transaction);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(transaction.Id, response.Id);
        Assert.Equal(10075, response.Amount);
        Assert.Equal("USD", response.Currency);
        Assert.Equal(5678, response.CardNumberLastFour);
        Assert.Equal(12, response.ExpiryMonth);
        Assert.Equal(2025, response.ExpiryYear);
        Assert.Equal(PaymentStatus.Authorized.ToString(), response.Status);
    }

    [Fact]
    public void Should_Map_PostPaymentRequest_To_MountebankRequest_Correctly()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            Amount = 100.75M,
            Currency = "USD",
            CardNumber = "1234567812345678",
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Cvv = 123
        };

        // Act
        var mountebankRequest = _mapper.Map<MountebankRequest>(request);

        // Assert
        Assert.NotNull(mountebankRequest);
        Assert.Equal(10075, mountebankRequest.Amount);
        Assert.Equal("123", mountebankRequest.Cvv);
        Assert.Equal("1234567812345678", mountebankRequest.CardNumber);
        Assert.Equal("USD", mountebankRequest.Currency);
        Assert.Equal("12/2025", mountebankRequest.ExpiryDate); 
    }

    [Fact]
    public void Should_Handle_Empty_PostPaymentRequest_Gracefully()
    {
        // Arrange
        var request = new PostPaymentRequest();

        // Act
        var transaction = _mapper.Map<Transaction>(request);

        // Assert
        Assert.NotNull(transaction);
        Assert.Equal(0, transaction.Amount);
        Assert.Null(transaction.Currency);
        Assert.Null(transaction.Merchant);
        Assert.Equal(0, transaction.CardNumberLastFour);
        Assert.Equal(0, transaction.ExpiryMonth);
        Assert.Equal(0, transaction.ExpiryYear);
    }

    [Fact]
    public void Should_Handle_Empty_Transaction_Gracefully()
    {
        // Arrange
        var transaction = new Transaction();

        // Act
        var response = _mapper.Map<PostPaymentResponse>(transaction);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(Guid.Empty, response.Id);
        Assert.Equal(0, response.Amount);
        Assert.Null(response.Currency);
        Assert.Equal(0, response.CardNumberLastFour);
        Assert.Equal(0, response.ExpiryMonth);
        Assert.Equal(0, response.ExpiryYear);
        Assert.Equal(PaymentStatus.Requested.ToString(), response.Status);
    }

    [Fact]
    public void Should_Handle_Null_Values_In_Transaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = 0,
            Currency = null,
            Merchant = null,
            CardNumberLastFour = 0,
            ExpiryMonth = 0,
            ExpiryYear = 0,
            Status = PaymentStatus.Rejected
        };

        // Act
        var response = _mapper.Map<PostPaymentResponse>(transaction);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(transaction.Id, response.Id);
        Assert.Equal(0, response.Amount);
        Assert.Null(response.Currency);
        Assert.Equal(0, response.CardNumberLastFour);
        Assert.Equal(0, response.ExpiryMonth);
        Assert.Equal(0, response.ExpiryYear);
        Assert.Equal("Rejected", response.Status);
    }
}
