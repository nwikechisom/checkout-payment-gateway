
using AutoMapper;

using PaymentGateway.Api.Data.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Utils;

namespace PaymentGateway.Api.Models.Mapping;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<PostPaymentRequest, Transaction>()
            .ForMember(t => t.Id, t => t.Ignore())
            .ForMember(t => t.Status, t => t.Ignore())
            .ForMember(t => t.Amount, t => t.MapFrom(p => p.Amount.ToMinorCurrencyUnit()))
            .ForMember(t => t.Currency, t => t.MapFrom(p => p.Currency))
            .ForMember(t => t.Merchant, t => t.MapFrom(p => p.MerchantId))
            .ForMember(t => t.CardNumberLastFour, t => t.MapFrom(p => p.CardNumber.GetLastFourDigits()))
            .ForMember(t => t.ExpiryMonth, t => t.MapFrom(p => p.ExpiryMonth))
            .ForMember(t => t.ExpiryYear, t => t.MapFrom(p => p.ExpiryYear));
        CreateMap<Transaction, PostPaymentResponse>()
            .ForMember(p => p.Id, p => p.MapFrom(t => t.Id))
            .ForMember(p => p.Amount, p => p.MapFrom(t => t.Amount))
            .ForMember(p => p.CardNumberLastFour, p => p.MapFrom(t => t.CardNumberLastFour))
            .ForMember(p => p.Currency, p => p.MapFrom(t => t.Currency))
            .ForMember(p => p.ExpiryMonth, p => p.MapFrom(t => t.ExpiryMonth))
            .ForMember(p => p.ExpiryYear, p => p.MapFrom(t => t.ExpiryYear))
            .ForMember(p => p.Status, p => p.MapFrom(t => t.Status.ToString()))
            .ForMember(p => p.ResponseMessage, p => p.Ignore());
        CreateMap<PostPaymentRequest, MountebankRequest>()
            .ForMember(m => m.Amount, p => p.MapFrom(p => p.Amount.ToMinorCurrencyUnit()))
            .ForMember(m => m.Cvv, p => p.MapFrom(p => p.Cvv))
            .ForMember(m => m.CardNumber, p => p.MapFrom(p => p.CardNumber))
            .ForMember(m => m.ExpiryDate, p => p.MapFrom(p => Utilities.GenerateExpiryDate(p.ExpiryYear, p.ExpiryMonth)))
            .ForMember(m => m.Currency, p => p.MapFrom(p => p.Currency));
    }
}