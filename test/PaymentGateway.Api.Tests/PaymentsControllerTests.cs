using System.Net;
using System.Net.Http.Json;
using System.Text;

using FluentValidation;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using Moq;

using Newtonsoft.Json;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Data.Context;
using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Random _random = new();

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var expectedPayment = new PostPaymentResponse
        {
            Id = paymentId,
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        // var paymentsRepository = new PaymentsRepository();
        // paymentsRepository.Add(payment);
        //
        // var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        // var client = webApplicationFactory.WithWebHostBuilder(builder =>
        //     builder.ConfigureServices(services => ((ServiceCollection)services)
        //         .AddSingleton(paymentsRepository)))
        //     .CreateClient();
        
        var mockPaymentsService = new Mock<IPaymentsService>();
        mockPaymentsService.Setup(service => service.GetPayment(paymentId)).Returns(expectedPayment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(mockPaymentsService.Object);
                }))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{paymentId}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task PostPayment_ReturnsAuthorized_WhenPaymentIsProcessed()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            MerchantId = "8987",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 1,
            Cvv = 123
        };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("api/payments", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var paymentResponse = JsonConvert.DeserializeObject<PostPaymentResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Authorized.ToString(), paymentResponse.Status);
        // Assert.Equal(100.00m, paymentResponse.Amount); 
    }

    [Fact]
    public async Task PostPayment_ReturnsDeclined_WhenPaymentIsProcessed()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "2222405343248112",
            MerchantId = "8987",
            ExpiryMonth = 1,
            ExpiryYear = 2026,
            Currency = "USD",
            Amount = 600,
            Cvv = 456
        };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var paymentResponse = JsonConvert.DeserializeObject<PostPaymentResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Declined.ToString(), paymentResponse.Status);
    }

    [Fact]
    public async Task PostPayment_ReturnsRejected_WhenPaymentIsProcessed()
    {
        // Arrange
        var request = new PostPaymentRequest
        {
            CardNumber = "22224053432877",
            MerchantId = "8987",
            ExpiryMonth = 0,
            ExpiryYear = 205,
            Currency = "GBP",
            Amount = 1,
            Cvv = 123
        };
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("api/payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var paymentResponse = JsonConvert.DeserializeObject<PostPaymentResponse>(await response.Content.ReadAsStringAsync());
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected.ToString(), paymentResponse.Status);
    }
}