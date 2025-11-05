using EVBTradingContract.Common;
using EVBTradingContract.Request;
using EVBTradingContract.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Repositories.Repository;   // OrderRepository, PaymentRepository
using Services.Interface;
using System;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using static Repositories.Enum.Enum; // dùng OrderStatus/PaymentStatus/PaymentMethod

namespace Services.Implement;

public sealed class VNPayService : IVNPayService
{
    private readonly ILogger<VNPayService> _logger;
    private readonly IVnpay _vnpay;
    private readonly IHttpContextAccessor _http;
    private readonly OrderRepository _orderRepository;       // repo sẵn có trong EVB
    private readonly PaymentRepository _paymentRepository;   // repo sẵn có trong EVB

    public VNPayService(
        ILogger<VNPayService> logger,
        IVnpay vnpay,
        IConfiguration cfg,
        IHttpContextAccessor http,
        OrderRepository orderRepository,
        PaymentRepository paymentRepository)
    {
        _logger = logger;
        _vnpay = vnpay;
        _http = http;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;

        // Init VNPay 1 lần – dùng cấu hình của bạn
        _vnpay.Initialize(
            cfg["Vnpay:TmnCode"]!,
            cfg["Vnpay:HashSecret"]!,
            cfg["Vnpay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            cfg["Vnpay:ReturnUrl"]!
        );
    }

    public async Task<ApiResponse<VNPayCreateResponse>> GetPaymentUrl(Guid orderId)
    {
        _logger.LogInformation("VNPay.GetPaymentUrl order={OrderId}", orderId);

        // a) Lấy order & check trạng thái (enum compare)
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.Status != OrderStatus.Pending)
            return Fail<VNPayCreateResponse>("Order not found or not in pending status");

        // b) Lấy số tiền (price từ Listing)
        var amountVnd = await _orderRepository.GetListingPriceByOrderIdAsync(orderId);
        if (!amountVnd.HasValue || amountVnd.Value <= 0)
            return Fail<VNPayCreateResponse>("Listing price not found or invalid");

        // c) Ghi/đặt Payment = Pending (repo của bạn có sẵn)
        await _paymentRepository.UpsertPendingAsync(orderId, amountVnd.Value);  // đã SaveChanges bên trong :contentReference[oaicite:5]{index=5}

        // d) Tự dò IP
        var ip = _http.HttpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault()
              ?? _http.HttpContext?.Request?.Headers["X-Real-IP"].FirstOrDefault()
              ?? _http.HttpContext?.Connection?.RemoteIpAddress?.ToString()
              ?? "0.0.0.0";

        // e) Tạo URL (để ANY cho flow “Try It Now”)
        var req = new PaymentRequest
        {
            PaymentId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Money = Convert.ToDouble(amountVnd.Value),
            Description = orderId.ToString(), // callback đọc lại orderId
            IpAddress = ip,
            BankCode = BankCode.ANY,
            CreatedDate = DateTime.Now,
            Currency = Currency.VND,
            Language = DisplayLanguage.Vietnamese
        };

        var url = _vnpay.GetPaymentUrl(req);
        if (string.IsNullOrWhiteSpace(url))
            return Fail<VNPayCreateResponse>("VNPay did not return a payment URL");

        _logger.LogInformation("VNPay.GetPaymentUrl OK order={OrderId}", orderId);
        return Ok(new VNPayCreateResponse { PaymentUrl = url }, "Successfully get payment url"); // :contentReference[oaicite:6]{index=6}
    }

    // 2) Xử lý callback / IPN
    public async Task<ApiResponse<VNPayReturnResponse>> ProcessIpnAction(IQueryCollection query)
    {
        var result = _vnpay.GetPaymentResult(query);
        var resp = new VNPayReturnResponse
        {
            Success = result.IsSuccess,
            Message = result.PaymentResponse.Description,
            OrderId = result.Description,
            Amount = query["vnp_Amount"].FirstOrDefault(),
            TransactionNo = query["vnp_TransactionNo"].FirstOrDefault(),
            ResponseCode = query["vnp_ResponseCode"].FirstOrDefault(),
            TransactionStatus = query["vnp_TransactionStatus"].FirstOrDefault()
        };

        if (!result.IsSuccess)
            return Fail(resp, result.PaymentResponse.Description); // trả Data kèm message rõ

        // Lấy orderId từ Description
        if (!Guid.TryParse(result.Description, out var orderId))
            return Fail(resp, "Invalid order id in VNPay description");

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Fail(resp, $"Order {orderId} not found");

        // vnp_Amount là số *100*
        var paidVnd = 0m;
        if (decimal.TryParse(resp.Amount, out var raw))
            paidVnd = raw / 100m;

        // Lấy/ghi Payment
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId)
                   ?? new Payment { Id = Guid.NewGuid(), OrderId = orderId, CreatedAt = DateTime.UtcNow };

        payment.AmountVnd = paidVnd > 0 ? paidVnd : payment.AmountVnd;
        payment.Method = PaymentMethod.VnPay;
        payment.Status = PaymentStatus.Success;
        payment.ProviderTxnId = resp.TransactionNo;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        // Repo của bạn: UpdateAsync có sẵn (tự SaveChanges) 
        if (await _paymentRepository.GetByOrderIdAsync(orderId) is null)
            await _paymentRepository.UpdateAsync(payment);

        await _orderRepository.UpdateAsync(order);

        return Ok(resp, "IPN processed successfully");
    }

    // ---- helpers chuẩn hóa ApiResponse<T> ----
    private static ApiResponse<T> Ok<T>(T data, string message = "")
        => new ApiResponse<T> { Success = true, Message = string.IsNullOrWhiteSpace(message) ? "OK" : message, Data = data };

    private static ApiResponse<T> Fail<T>(string message)
        => new ApiResponse<T> { Success = false, Message = message, Data = default };

    private static ApiResponse<T> Fail<T>(T data, string message)
        => new ApiResponse<T> { Success = false, Message = message, Data = data };
}