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

        public OrderService(OrderRepository repo, ILogger<OrderService> logger)
        {
            _repo = repo;
            _logger = logger;
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
