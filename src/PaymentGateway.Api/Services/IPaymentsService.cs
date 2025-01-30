using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsService
{
    Task<PostPaymentResponse> PostPayment(PostPaymentRequest request, string idempotencyKey);
    PostPaymentResponse GetPayment(Guid id);
}