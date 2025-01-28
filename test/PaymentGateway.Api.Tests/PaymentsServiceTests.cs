using System.Net;
using System.Text.Json;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Utils;

namespace PaymentGateway.Api.Tests;

public class PaymentsServiceTests
{
    private readonly Mock<IValidator<PostPaymentRequest>> _validatorMock = new();
    private readonly Mock<IPaymentsRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<PaymentsService>> _loggerMock = new();
    private readonly IOptions<MounteBankConfig> _options;
    private readonly PaymentsService _service;

    public PaymentsServiceTests()
    {
        _options = Options.Create(new MounteBankConfig
        {
            BaseUrl = "http://mockbank.com",
            PostPaymentEndpoint = "/api/payments"
        });

        _service = new PaymentsService(_options, _validatorMock.Object, _repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PostPayment_ShouldReturnRejectedResponse_WhenValidationFails()
    {
        // Arrange
        var request = GetRequest();
        var transaction = GetTransaction();
        
        var validationFailures = new List<ValidationFailure>
        {
            new("Property", "Error message")
        };
        _validatorMock.Setup(v => v.Validate(request))
            .Returns(new ValidationResult(validationFailures));
        _mapperMock.Setup(m => m.Map<Transaction>(request)).Returns(transaction);
        _repositoryMock.Setup(r => r.Add(It.IsAny<Transaction>())).ReturnsAsync(transaction);
        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(transaction)).Returns(new PostPaymentResponse
        {    
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 1,
        });

        // Act
        var response = await _service.PostPayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, transaction.Status);
        Assert.Contains("Error message", response.ResponseMessage);
    }

    [Fact]
    public async Task PostPayment_ShouldReturnDeclinedResponse_WhenBankDeclinesPayment()
    {
        // Arrange
        var request = GetRequest();
        var transaction = GetTransaction();
        _validatorMock.Setup(v => v.Validate(request))
            .Returns(new ValidationResult());

        _mapperMock.Setup(m => m.Map<Transaction>(request)).Returns(transaction);
        _repositoryMock.Setup(r => r.Add(It.IsAny<Transaction>())).ReturnsAsync(transaction);
        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(transaction)).Returns(new PostPaymentResponse
        {    
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 1,
        });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new MountebankResponse { Authorized = false }))
        };

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var client = new HttpClient(httpClientMock.Object);
        typeof(PaymentsService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_service, client);

        // Act
        var response = await _service.PostPayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Declined, transaction.Status);
        Assert.Equal("Payment was declined", response.ResponseMessage);
    }
    
    [Fact]
    public async Task PostPayment_ShouldReturnAuthorizedResponse_WhenPaymentIsSuccessful()
    {
        // Arrange
        var request = GetRequest();
        var transaction = GetTransaction();
        _validatorMock.Setup(v => v.Validate(request))
            .Returns(new ValidationResult());

        _mapperMock.Setup(m => m.Map<Transaction>(request)).Returns(transaction);
        _repositoryMock.Setup(r => r.Add(It.IsAny<Transaction>())).ReturnsAsync(transaction);
        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(transaction)).Returns(new PostPaymentResponse
        {    
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 1,
        });
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new MountebankResponse { Authorized = true }))
        };

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var client = new HttpClient(httpClientMock.Object);
        typeof(PaymentsService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_service, client);

        // Act
        var response = await _service.PostPayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, transaction.Status);
        Assert.NotNull(response);
    }

    [Fact]
    public void GetPayment_ShouldReturnPaymentResponse_WhenTransactionExists()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transaction = new Transaction { Id = transactionId };
        _repositoryMock.Setup(r => r.Get(transactionId)).Returns(transaction);

        var response = new PostPaymentResponse();
        _mapperMock.Setup(m => m.Map<PostPaymentResponse>(transaction)).Returns(response);

        // Act
        var result = _service.GetPayment(transactionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(response, result);
    }

    [Fact]
    public void GetPayment_ShouldReturnNull_WhenTransactionDoesNotExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.Get(transactionId)).Returns((Transaction)null);

        // Act
        var result = _service.GetPayment(transactionId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PostPayment_ShouldHandleMappingExceptionGracefully()
    {
        // Arrange
        var request = new PostPaymentRequest();
    
        // Simulate a mapping failure
        _mapperMock.Setup(m => m.Map<Transaction>(request)).Throws(new AutoMapperMappingException("Mapping failed"));

        // Act
        var response = await _service.PostPayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected.ToString(), response.Status);
        Assert.Equal("Mapping error occurred.", response.ResponseMessage);
    }
    
    [Fact]
    public async Task PostPayment_ShouldHandleHttpClientExceptionGracefully()
    {
        // Arrange
        var request = new PostPaymentRequest();
        _validatorMock.Setup(v => v.Validate(request))
            .Returns(new ValidationResult());

        var transaction = new Transaction();
        _mapperMock.Setup(m => m.Map<Transaction>(request)).Returns(transaction);
        _repositoryMock.Setup(r => r.Add(It.IsAny<Transaction>())).ReturnsAsync(transaction);

        // Mock HttpClient to throw HttpRequestException
        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Throws(new HttpRequestException("HTTP request failed"));

        var client = new HttpClient(httpClientMock.Object);
        typeof(PaymentsService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_service, client);

        // Act
        var response = await _service.PostPayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected.ToString(), response.Status);
        Assert.Equal("Network error occurred while processing payment.", response.ResponseMessage);
    }

    [Fact]
    public async Task PostPayment_ShouldHandleJsonExceptionGracefully()
    {
        // Arrange
        var request = new PostPaymentRequest();
        _validatorMock.Setup(v => v.Validate(request))
            .Returns(new ValidationResult());

        var transaction = new Transaction();
        _mapperMock.Setup(m => m.Map<Transaction>(request)).Returns(transaction);
        _repositoryMock.Setup(r => r.Add(It.IsAny<Transaction>())).ReturnsAsync(transaction);

        // Mock HttpClient to return a bad JSON response
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Invalid JSON")
        };

        var httpClientMock = new Mock<HttpMessageHandler>();
        httpClientMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);

        var client = new HttpClient(httpClientMock.Object);
        typeof(PaymentsService).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_service, client);

        // Act
        var response = await _service.PostPayment(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected.ToString(), response.Status);
        Assert.Equal("Invalid response format from payment gateway.", response.ResponseMessage);
    }
    
    
    private PostPaymentRequest GetRequest()
    {
        return new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            MerchantId = "8987",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 1,
            Cvv = 123
        };
    }

    private Transaction GetTransaction()
    {
        return new Transaction
        {
            Merchant = "8987",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 1,
        };
    }
}