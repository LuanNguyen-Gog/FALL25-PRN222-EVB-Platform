using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Repositories.Repository;
using Services.Helpers;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using static Repositories.Enum.Enum;

namespace Services.Implement
{
    public class VNPayService : IVNPayService
    {
        private readonly ILogger<VNPayService> _logger;
        private readonly PaymentRepository _paymentRepo;
        private readonly OrderRepository _orderRepo;
        private readonly IConfiguration _cfg;
        public VNPayService(ILogger<VNPayService> logger, PaymentRepository paymentRepo, OrderRepository orderRepo, IConfiguration cfg)
        {
            _logger = logger;
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _cfg = cfg;
        }
        public async Task<ApiResponse<VNPayCreateResponse>> CreatePaymentUrlAsync(Guid orderId, VNPayCreateRequest request, CancellationToken ct = default)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    return new ApiResponse<VNPayCreateResponse> { Success = false, Message = "Order not found" };

                // Chỉ cho thanh toán khi Order đang Pending (khớp enum)
                if (order.Status != OrderStatus.Pending)
                    return new ApiResponse<VNPayCreateResponse> { Success = false, Message = $"Order status must be Pending (current: {order.Status})" };

                var amount = request.AmountOverride ?? order.Listing?.PriceVnd ?? 0m; // nếu bạn muốn lấy từ Listing/Order tuỳ logic
                if (amount <= 0)
                    return new ApiResponse<VNPayCreateResponse> { Success = false, Message = "Invalid amount" };

                var now = DateTime.UtcNow;

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    AmountVnd = amount,
                    Status = PaymentStatus.Pending,
                    Method = PaymentMethod.VnPay,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _paymentRepo.CreateAsync(payment, ct);

                // Build URL VNPay
                var tmnCode = _cfg["VnPay:TmnCode"] ?? throw new InvalidOperationException("Missing VnPay:TmnCode");
                var hashSecret = _cfg["VnPay:HashSecret"] ?? throw new InvalidOperationException("Missing VnPay:HashSecret");
                var returnUrl = _cfg["VnPay:ReturnUrl"] ?? throw new InvalidOperationException("Missing VnPay:ReturnUrl");
                var ipnUrl = _cfg["VnPay:IpnUrl"] ?? throw new InvalidOperationException("Missing VnPay:IpnUrl");
                var baseUrl = _cfg["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

                var payUrl = VNPayHelper.BuildPaymentUrl(
                    baseUrl: _cfg["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
                    tmnCode: _cfg["VnPay:TmnCode"] ?? throw new InvalidOperationException("Missing VnPay:TmnCode"),
                    hashSecret: _cfg["VnPay:HashSecret"] ?? throw new InvalidOperationException("Missing VnPay:HashSecret"),
                    returnUrl: _cfg["VnPay:ReturnUrl"] ?? throw new InvalidOperationException("Missing VnPay:ReturnUrl"),
                    ipnUrl: _cfg["VnPay:IpnUrl"] ?? "",
                    amountTimes100: (long)(amount * 100),
                    txnRef: payment.Id.ToString(),
                    orderInfo: $"EVB Order {order.Id}",
                    clientIp: request.ClientIp ?? "0.0.0.0",
                    expireUtc: DateTime.UtcNow.AddMinutes(15)
                );

                return new ApiResponse<VNPayCreateResponse>
                {
                    Success = true,
                    Message = "OK",
                    Data = new VNPayCreateResponse
                    {
                        PaymentId = payment.Id,
                        OrderId = order.Id,
                        AmountVnd = amount,
                        PaymentUrl = payUrl
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePaymentUrlAsync error");
                return new ApiResponse<VNPayCreateResponse> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<ApiResponse<VNPayReturnResponse>> HandleReturnAsync(IQueryCollection query, CancellationToken ct = default)
        {
            try
            {
                var hashSecret = _cfg["VnPay:HashSecret"] ?? throw new InvalidOperationException("Missing VnPay:HashSecret");

                if (!VNPayHelper.ValidateReturn(query, hashSecret, out var respData))
                {
                    return new ApiResponse<VNPayReturnResponse>
                    {
                        Success = false,
                        Message = "Invalid signature",
                        Data = new VNPayReturnResponse { IsSuccess = false, Message = "Invalid signature" }
                    };
                }

                // Đọc giá trị từ dữ liệu trả về
                var rspCode = VNPayHelper.Get(respData, "vnp_ResponseCode"); // "00" = thành công
                var txnRef = VNPayHelper.Get(respData, "vnp_TxnRef");       // ID Payment
                var bankTxnNo = VNPayHelper.Get(respData, "vnp_BankTranNo");

                if (!Guid.TryParse(txnRef, out var paymentId))
                {
                    return new ApiResponse<VNPayReturnResponse>
                    {
                        Success = false,
                        Message = "Invalid txnRef",
                        Data = new VNPayReturnResponse { IsSuccess = false, Message = "Invalid txnRef" }
                    };
                }

                // Lấy số tiền đã thanh toán từ VNPay
                var payment = await _paymentRepo.GetByIdAsync(paymentId);
                if (payment == null)
                    return new ApiResponse<VNPayReturnResponse> { Success = false, Message = "Payment not found" };

                var order = await _orderRepo.GetByIdAsync(payment.OrderId);
                if (order == null)
                    return new ApiResponse<VNPayReturnResponse> { Success = false, Message = "Order not found" };

                var data = new VNPayReturnResponse
                {
                    RspCode = rspCode,
                    VnPayTxnNo = bankTxnNo
                };

                // 3) Xử lý kết quả
                if (rspCode == "00")
                {
                    var paidAmount = VNPayHelper.GetAmountTimes100(respData) / 100m;
                    if (paidAmount != payment.AmountVnd)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.ProviderTxnId = bankTxnNo;
                        payment.PaidAt = null;
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepo.UpdateAsync(payment);

                        data.IsSuccess = false;
                        data.Message = "Amount mismatch";
                        return new ApiResponse<VNPayReturnResponse> { Success = false, Message = data.Message!, Data = data };
                    }

                    // success
                    payment.Status = PaymentStatus.Success;
                    payment.ProviderTxnId = bankTxnNo;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _paymentRepo.UpdateAsync(payment);

                    // tuỳ business: Completed/Processing
                    order.Status = OrderStatus.Completed;
                    await _orderRepo.UpdateAsync(order);

                    data.IsSuccess = true;
                    data.Message = "Payment success";
                    data.ProviderTxnId = bankTxnNo;

                    return new ApiResponse<VNPayReturnResponse> { Success = true, Message = "OK", Data = data };
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.ProviderTxnId = bankTxnNo;
                    payment.PaidAt = null;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _paymentRepo.UpdateAsync(payment);

                    data.IsSuccess = false;
                    data.Message = $"VNPay failed ({rspCode})";
                    return new ApiResponse<VNPayReturnResponse> { Success = false, Message = data.Message!, Data = data };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HandleReturnAsync error");
                return new ApiResponse<VNPayReturnResponse> { Success = false, Message = ex.Message, Data = null };
            }
        }

        public async Task<string> HandleIpnAsync(IQueryCollection query, CancellationToken ct = default)
        {
            try
            {
                var hashSecret = _cfg["VnPay:HashSecret"]
                         ?? throw new InvalidOperationException("Missing VnPay:HashSecret");

                if (!VNPayHelper.ValidateReturn(query, hashSecret, out var respData))
                    return "INVALID";

                var rspCode = VNPayHelper.Get(respData, "vnp_ResponseCode");
                var txnRef = VNPayHelper.Get(respData, "vnp_TxnRef");
                var bankTxnNo = VNPayHelper.Get(respData, "vnp_BankTranNo");

                if (!Guid.TryParse(txnRef, out var paymentId)) return "INVALID";

                var payment = await _paymentRepo.GetByIdAsync(paymentId);
                if (payment == null) return "INVALID";

                var order = await _orderRepo.GetByIdAsync(payment.OrderId);
                if (order == null) return "INVALID";

                if (rspCode == "00")
                {
                    var paidAmount = VNPayHelper.GetAmountTimes100(respData) / 100m;
                    if (paidAmount != payment.AmountVnd)
                    {
                        payment.Status = PaymentStatus.Failed;
                        payment.ProviderTxnId = bankTxnNo;
                        payment.PaidAt = null;
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepo.UpdateAsync(payment);
                        return "INVALID";
                    }

                    if (payment.Status != PaymentStatus.Success)
                    {
                        payment.Status = PaymentStatus.Success;
                        payment.ProviderTxnId = bankTxnNo;
                        payment.PaidAt = DateTime.UtcNow;
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepo.UpdateAsync(payment);

                        order.Status = OrderStatus.Completed;
                        await _orderRepo.UpdateAsync(order);
                    }
                    return "OK";
                }

                payment.Status = PaymentStatus.Failed;
                payment.ProviderTxnId = bankTxnNo;
                payment.PaidAt = null;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepo.UpdateAsync(payment);
                return "OK";
            }
            catch
            {
                return "INVALID";
            }
        }
    }
}