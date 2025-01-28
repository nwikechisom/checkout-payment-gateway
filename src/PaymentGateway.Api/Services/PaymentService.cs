using System.Text.Json;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Options;
using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsService(IOptions<MounteBankConfig> options, 
    IValidator<PostPaymentRequest> validator, 
    IPaymentsRepository paymentsRepository,
    IMapper mapper, ILogger<PaymentsService> logger) : IPaymentsService
{
    private readonly MounteBankConfig _bankConfig = options.Value;
    private readonly HttpClient _httpClient = new();
    
    public async Task<PostPaymentResponse> PostPayment(PostPaymentRequest request)
{
    try
    {
        var mapping = mapper.Map<Transaction>(request);
        var transaction = await paymentsRepository.Add(mapping);
        
        // Validate the request
        var validationResult = validator.Validate(request);
        if (validationResult.Errors.Any())
        {
            var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
            transaction.Status = PaymentStatus.Rejected;
            await paymentsRepository.Update(transaction);
            
            var rejectedResponse = mapper.Map<PostPaymentResponse>(transaction);
            rejectedResponse.ResponseMessage = string.Join("\n", errors);
            return rejectedResponse;
        }

        var mounteBankRequest = mapper.Map<MountebankRequest>(request);
        var result = await _httpClient
            .PostAsJsonAsync(_bankConfig.BaseUrl + _bankConfig.PostPaymentEndpoint, mounteBankRequest);
        var stringResult = await result.Content.ReadAsStringAsync();
        result.EnsureSuccessStatusCode(); 
        // var stringResult = await result.Content.ReadAsStringAsync();
        var mountebankResponse = JsonSerializer.Deserialize<MountebankResponse>(stringResult);

        if (mountebankResponse?.Authorized == false)
        {
            transaction.Status = PaymentStatus.Declined;
            await paymentsRepository.Update(transaction);
            var declinedResponse = mapper.Map<PostPaymentResponse>(transaction);
            declinedResponse.ResponseMessage = "Payment was declined";
            return declinedResponse;
        }

        // If the payment is authorized, update the status
        transaction.Status = PaymentStatus.Authorized;
        await paymentsRepository.Update(transaction);
        
        return mapper.Map<PostPaymentResponse>(transaction);
    }
    catch (HttpRequestException ex)
    {
        // Handle HTTP errors (e.g., connection issues, timeout, etc.)
        logger.LogError(ex, "HTTP request failed when posting payment.");
        var errorResponse = new PostPaymentResponse
        {
            Status = PaymentStatus.Rejected.ToString(),
            ResponseMessage = "Network error occurred while processing payment."
        };
        return errorResponse;
    }
    catch (JsonException ex)
    {
        logger.LogError(ex, "Error deserializing response from the payment gateway.");

        var errorResponse = new PostPaymentResponse
        {
            Status = PaymentStatus.Rejected.ToString(),
            ResponseMessage = "Invalid response format from payment gateway."
        };
        return errorResponse;
    }
    catch (AutoMapperMappingException ex)
    {
        logger.LogError(ex, "Error mapping request or response object.");
        var errorResponse = new PostPaymentResponse
        {
            Status = PaymentStatus.Rejected.ToString(),
            ResponseMessage = "Mapping error occurred."
        };
        return errorResponse;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An unexpected error occurred while processing payment.");
        var errorResponse = new PostPaymentResponse
        {
            Status = PaymentStatus.Rejected.ToString(),
            ResponseMessage = "An unexpected error occurred."
        };
        return errorResponse;
    }
}


    public PostPaymentResponse GetPayment(Guid id)
    {
        var transaction = paymentsRepository.Get(id);
        // if (transaction == null) return null;
        return mapper.Map<PostPaymentResponse>(transaction);
    }
}