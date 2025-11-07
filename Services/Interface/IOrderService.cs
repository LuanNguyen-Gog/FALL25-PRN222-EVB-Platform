using EVBTradingContract.Common;
using EVBTradingContract.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repositories.Enum.Enum;

namespace Services.Interface
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderResponse>> CreateAsync(Guid buyerId, Guid listingId, CancellationToken ct = default);
        Task<ApiResponse<OrderResponse>> GetAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse<object>> SearchAsync(Guid? buyerId, string? status, int page, int pageSize, CancellationToken ct = default);
        Task<ApiResponse<OrderResponse>> UpdateStatusAsync(Guid id, OrderStatus newStatus, CancellationToken ct = default);
        Task<ApiResponse<OrderResponse>> OnPaymentSucceededAsync(Guid orderId, CancellationToken ct = default);
        Task<ApiResponse<OrderResponse>> OnPaymentFailedAsync(Guid orderId, CancellationToken ct = default);
        Task<ApiResponse<OrderAndContractResponse>> ConfirmPaymentSuccessAsync(
        Guid orderId, CancellationToken ct = default);
        Task<ApiResponse<OrderAndContractResponse>> HandleContractAcceptedAsync(Guid orderId, CancellationToken ct = default);
        Task<ApiResponse<OrderAndContractResponse>> HandleContractCancelledAsync(Guid orderId, string? reason, CancellationToken ct = default);
    }
}
