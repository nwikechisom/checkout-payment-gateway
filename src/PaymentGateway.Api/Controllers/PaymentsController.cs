﻿using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentsService paymentsService) : Controller
{
    [HttpGet("{id:guid}")]
    public ActionResult<GetPaymentResponse?> GetPaymentAsync(Guid id)
    {
        var payment = paymentsService.GetPayment(id);
        if (payment is null) return NotFound();
        return new OkObjectResult(payment);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse?>> PostPayment([FromBody]PostPaymentRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            return BadRequest("Idempotency-Key header is required.");
        }
        var payment = await paymentsService.PostPayment(request, idempotencyKey);
        if (payment.Status != PaymentStatus.Authorized.ToString()) 
            return new BadRequestObjectResult(payment);
        return new OkObjectResult(payment);
    }
}