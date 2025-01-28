using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Data.Models;

public class Transaction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public int Amount { get; set; }
    public string Merchant { get; set; }
    public string Currency { get; set; }
    // public string Description { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Requested;
}