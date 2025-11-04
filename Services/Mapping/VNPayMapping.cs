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
            // Payment → VNPayCreateResponse
            TypeAdapterConfig<Payment, VNPayCreateResponse>
                .NewConfig()
                .Map(dest => dest.PaymentId, src => src.Id)
                .Map(dest => dest.OrderId, src => src.OrderId)
                .Map(dest => dest.AmountVnd, src => src.AmountVnd)
                .Ignore(dest => dest.PaymentUrl); // PaymentUrl tạo trong service (không có trong entity)

            // Payment → VNPayReturnResponse (trường hợp bạn cần map tự động)
            TypeAdapterConfig<Payment, VNPayReturnResponse>
                .NewConfig()
                .Map(dest => dest.IsSuccess, src => src.Status == PaymentStatus.Success)
                .Map(dest => dest.Message, src => src.Status == PaymentStatus.Success ? "Payment success" : "Payment failed")
                .Map(dest => dest.ProviderTxnId, src => src.ProviderTxnId)
                .Ignore(dest => dest.RspCode)
                .Ignore(dest => dest.VnPayTxnNo);
        }
    }
}
