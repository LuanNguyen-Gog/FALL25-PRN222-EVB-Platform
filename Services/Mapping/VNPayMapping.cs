using EVBTradingContract.Response;
using Mapster;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNPAY.NET.Models;
using static Repositories.Enum.Enum;

namespace Services.Mapping
{
    public class VNPayMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Payment, VNPayReturnResponse>()
              .Map(dest => dest.Success, src => src.Status == PaymentStatus.Success)
              .Map(dest => dest.Message, src => src.Status == PaymentStatus.Success ? "Payment success" : "Payment failed")
              .Map(dest => dest.OrderId, src => src.OrderId.ToString())
              .Map(dest => dest.Amount, src => src.AmountVnd.ToString("0"))
              .Map(dest => dest.TransactionNo, src => src.ProviderTxnId)
              .Map(dest => dest.ResponseCode, src => (string?)null)
              .Map(dest => dest.TransactionStatus, src => src.Status.ToString());

            config.NewConfig<PaymentResult, VNPayReturnResponse>()
                  .Map(dest => dest.Success, src => src.IsSuccess)
                  .Map(dest => dest.Message, src => src.PaymentResponse.Description)
                  .Map(dest => dest.OrderId, src => src.Description)
                  .Map(dest => dest.Amount, src => (string?)null)
                  .Map(dest => dest.TransactionNo, src => (string?)null)
                  .Map(dest => dest.ResponseCode, src => (string?)null)
                  .Map(dest => dest.TransactionStatus, src => (string?)null);
        }
    }
}