using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Data.Models;

public class MountebankResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; set; }
    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; set; }
}

public class MountebankRequest
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; }
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    [JsonPropertyName("cvv")]
    public string Cvv { get; set; }
}