using EVBTradingContract.Common;
using EVBTradingContract.Response;
using Mapster;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Repositories.Repository;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repositories.Enum.Enum;

namespace Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepository _repo;
        private readonly ILogger<OrderService> _logger;
        private readonly PaymentRepository _payments;
        private readonly IContractService _contracts;
        private readonly ListingRepository _listings;

        public OrderService(OrderRepository repo, ILogger<OrderService> logger, PaymentRepository payments, IContractService contractService, ListingRepository listings)
        {
            _repo = repo;
            _logger = logger;
            _payments = payments;
            _contracts = contractService;
            _listings = listings;
        }

        public async Task<ApiResponse<OrderAndContractResponse>> ConfirmPaymentSuccessAsync(
        Guid orderId, CancellationToken ct = default)
        {
            // 1) Lấy order + giá listing
            var order = await _repo.GetByIdAsync(orderId);
            if (order is null)
                return new ApiResponse<OrderAndContractResponse> { Success = false, Message = "Order not found" };

            decimal? amountVnd = null;
            if (order.ListingId != Guid.Empty)
            {
                var listing = await _listings.GetByIdAsync(order.ListingId);
                amountVnd = listing?.PriceVnd;
            }

            // Upsert Payment = Success
            var p = await _payments.GetByOrderIdAsync(orderId);
            if (p is null)
            {
                p = new Payment { Id = Guid.NewGuid(), OrderId = orderId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                await _payments.CreateAsync(p, ct);
            }

            p.ProviderTxnId ??= $"VNP_SANDBOX_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            if (amountVnd is > 0) p.AmountVnd = Math.Round(amountVnd.Value, 0);

            p.Method = PaymentMethod.VnPay;
            p.Status = PaymentStatus.Success; // escrow: giữ tiền
            p.PaidAt ??= DateTime.UtcNow;
            p.UpdatedAt = DateTime.UtcNow;
            await _payments.UpdateAsync(p);

            // Order: Pending -> Processing
            var change = await OnPaymentSucceededAsync(orderId, ct); // (đã có sẵn) :contentReference[oaicite:3]{index=3}
            if (!change.Success)
                return new ApiResponse<OrderAndContractResponse> { Success = false, Message = change.Message ?? "Cannot move to Processing" };

            // Draft contract (chưa có file thì truyền "")
            var draft = await _contracts.UpsertForOrderAsync(orderId, fileUrl: "", ct); // không để null  :contentReference[oaicite:4]{index=4}

            var fresh = await _repo.GetByIdAsync(orderId); // dùng _repo, không phải _orders  :contentReference[oaicite:5]{index=5}

            var dto = new OrderAndContractResponse(
                new OrderMiniDto(fresh!.Id, fresh.Status.ToString()),
                new PaymentMiniDto(p.Id, p.Status.ToString(), p.ProviderTxnId, p.AmountVnd, p.PaidAt),
                draft.Success && draft.Data is not null
                    ? new ContractMiniDto(draft.Data.Id, draft.Data.Status.ToString() ?? "awaiting_buyer", draft.Data.ContractFileUrl, draft.Data.SignedAt)
                    : null
            );

            return new ApiResponse<OrderAndContractResponse> { Success = true, Message = "Payment confirmed (escrow)", Data = dto };
        }

        // Buyer ký thành công -> Completed (Payment giữ Success)
        public async Task<ApiResponse<OrderAndContractResponse>> HandleContractAcceptedAsync(Guid orderId, CancellationToken ct = default)
        {
            var signed = await _contracts.SignAsync(orderId, ct); // (đã có sẵn)  :contentReference[oaicite:6]{index=6}
            if (!signed.Success)
                return new ApiResponse<OrderAndContractResponse> { Success = false, Message = signed.Message ?? "Contract sign failed" };

            var done = await UpdateStatusAsync(orderId, OrderStatus.Completed, ct); // (đã có sẵn)  :contentReference[oaicite:7]{index=7}

            if (!done.Success)
                return new ApiResponse<OrderAndContractResponse> { Success = false, Message = done.Message ?? "Cannot complete order" };

            var pay = await _payments.GetByOrderIdAsync(orderId);
            var fresh = await _repo.GetByIdAsync(orderId);

            var dto = new OrderAndContractResponse(
                new OrderMiniDto(fresh!.Id, fresh.Status.ToString()),
                pay is null ? null : new PaymentMiniDto(pay.Id, pay.Status.ToString(), pay.ProviderTxnId, pay.AmountVnd, pay.PaidAt),
                signed.Data is null ? null : new ContractMiniDto(signed.Data.Id, signed.Data.Status.ToString() ?? "signed", signed.Data.ContractFileUrl, signed.Data.SignedAt)
            );

            return new ApiResponse<OrderAndContractResponse> { Success = true, Message = "Contract signed, order completed", Data = dto };
        }

        // Hợp đồng thất bại/huỷ -> Refund + Cancelled
        public async Task<ApiResponse<OrderAndContractResponse>> HandleContractCancelledAsync(Guid orderId, string? reason, CancellationToken ct = default)
        {
            // ⚠️ CẦN có CancelAsync trong ContractService
            var canceled = await _contracts.CancelAsync(orderId, reason, ct); // thêm hàm này ở ContractService (mục 3)  :contentReference[oaicite:8]{index=8}
            if (!canceled.Success)
                return new ApiResponse<OrderAndContractResponse> { Success = false, Message = canceled.Message ?? "Cannot cancel contract" };

            var p = await _payments.GetByOrderIdAsync(orderId);
            if (p is not null)
            {
                p.Status = PaymentStatus.Refunded; // sandbox: chỉ đổi trạng thái
                p.UpdatedAt = DateTime.UtcNow;
                await _payments.UpdateAsync(p);
            }

            var fail = await UpdateStatusAsync(orderId, OrderStatus.Cancelled, ct);
            if (!fail.Success)
                return new ApiResponse<OrderAndContractResponse> { Success = false, Message = fail.Message ?? "Cannot cancel order" };

            var fresh = await _repo.GetByIdAsync(orderId);
            var dto = new OrderAndContractResponse(
                new OrderMiniDto(fresh!.Id, fresh.Status.ToString()),
                p is null ? null : new PaymentMiniDto(p.Id, p.Status.ToString(), p.ProviderTxnId, p.AmountVnd, p.PaidAt),
                canceled.Data is null ? null : new ContractMiniDto(canceled.Data.Id, canceled.Data.Status.ToString() ?? "cancelled", canceled.Data.ContractFileUrl, canceled.Data.SignedAt)
            );

            return new ApiResponse<OrderAndContractResponse> { Success = true, Message = "Contract cancelled -> refunded", Data = dto };
        }

        // Create (dùng lại GenericRepo.CreateAsync)
        public async Task<ApiResponse<OrderResponse>> CreateAsync(Guid buyerId, Guid listingId, CancellationToken ct = default)
        {
            try
            {
                if (!await _repo.ListingExistsAsync(listingId, ct))
                    return new ApiResponse<OrderResponse> { Success = false, Message = "Listing not found" };

                var entity = new Order
                {
                    Id = Guid.NewGuid(),
                    BuyerId = buyerId,
                    ListingId = listingId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repo.CreateAsync(entity, ct); // kế thừa từ GenericRepo<T>
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order created",
                    Data = entity.Adapt<OrderResponse>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create order failed");
                return new ApiResponse<OrderResponse> { Success = false, Message = ex.Message };
            }
        }

        // Get detail (đặc thù: Include)
        public async Task<ApiResponse<OrderResponse>> GetAsync(Guid id, CancellationToken ct = default)
        {
            var o = await _repo.GetDetailAsync(id, asNoTracking: true, ct);
            if (o == null) return new ApiResponse<OrderResponse> { Success = false, Message = "Not found" };
            return new ApiResponse<OrderResponse> { Success = true, Data = o.Adapt<OrderResponse>() };
        }

        // Search (đặc thù)
        public async Task<ApiResponse<object>> SearchAsync(Guid? buyerId, string? status, int page, int pageSize, CancellationToken ct = default)
        {
            OrderStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var st))
                    parsedStatus = st;
                else
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Invalid status '{status}'. Allowed: pending, processing, completed, cancelled"
                    };
            }
            var (items, total) = await _repo.SearchAsync(buyerId, parsedStatus, page, pageSize, ct);
            var data = new
            {
                total,
                page,
                pageSize,
                items = items.Adapt<List<OrderResponse>>()
            };
            return new ApiResponse<object> { Success = true, Data = data };
        }

        public async Task<ApiResponse<OrderResponse>> UpdateStatusAsync(Guid id, OrderStatus newStatus, CancellationToken ct = default)
        {
            try
            {
                // dùng GenericRepo.GetByIdAsync(Guid) để lấy entity tracking
                var o = await _repo.GetByIdAsync(id);
                if (o == null) return new ApiResponse<OrderResponse> { Success = false, Message = "Order not found" };

                if (!CanTransition((OrderStatus)o.Status, newStatus))
                    return new ApiResponse<OrderResponse> { Success = false, Message = $"Invalid transition {o.Status} -> {newStatus}" };

                o.Status = newStatus;
                o.UpdatedAt = DateTime.UtcNow;

                await _repo.UpdateAsync(o);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order status updated",
                    Data = o.Adapt<OrderResponse>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update order status failed");
                return new ApiResponse<OrderResponse> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<OrderResponse>> OnPaymentSucceededAsync(Guid orderId, CancellationToken ct = default)
        {
            // pending -> processing (nếu muốn completed thì chỉnh target)
            var get = await _repo.GetByIdAsync(orderId);
            if (get == null) return new ApiResponse<OrderResponse> { Success = false, Message = "Order not found" };

            var target = get.Status == OrderStatus.Pending ? OrderStatus.Processing : get.Status;
            return await UpdateStatusAsync(orderId, (OrderStatus)target, ct);
        }

        public async Task<ApiResponse<OrderResponse>> OnPaymentFailedAsync(Guid orderId, CancellationToken ct = default)
            => await UpdateStatusAsync(orderId, OrderStatus.Cancelled, ct);

        // ---- RULE: state machine ----
        private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
        {
            [OrderStatus.Pending] = new[] { OrderStatus.Processing, OrderStatus.Cancelled },
            [OrderStatus.Processing] = new[] { OrderStatus.Completed, OrderStatus.Cancelled },
            [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
        };
        private static bool CanTransition(OrderStatus from, OrderStatus to) =>
            Allowed.TryGetValue(from, out var nexts) && nexts.Contains(to);
    } 
}

