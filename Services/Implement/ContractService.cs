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
    public class ContractService : IContractService
    {
        private readonly ContractRepository _repo;
        private readonly OrderRepository _orderRepo;
        private readonly ILogger<ContractService> _logger;

        public ContractService(ContractRepository repo, OrderRepository orderRepo, ILogger<ContractService> logger)
        {
            _repo = repo;
            _orderRepo = orderRepo;
            _logger = logger;
        }
        public async Task<ApiResponse<ContractResponse>> UpsertForOrderAsync(Guid orderId, string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    return new ApiResponse<ContractResponse>()
                    {
                        Success = false,
                        Message = "Order not found",
                        Data = null,
                    };

                if (order.Status == Repositories.Enum.Enum.OrderStatus.Cancelled)
                    return new ApiResponse<ContractResponse>()
                    {
                        Success = false,
                        Message = "Cannot create contract for cancelled order",
                        Data = null,
                    };

                var existing = await _repo.GetTrackingByOrderIdAsync(orderId, ct);
                if (existing == null)
                {
                    existing = new Contract
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        ContractFileUrl = fileUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _repo.CreateAsync(existing);
                }
                else
                {
                    if (existing.SignedAt != null)
                        return new ApiResponse<ContractResponse>() { Success = false, Message = "Contract already signed", Data = null };

                    existing.ContractFileUrl = fileUrl;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _repo.UpdateAsync(existing);
                }

                var contract = existing.Adapt<ContractResponse>();

                return new ApiResponse<ContractResponse>()
                {
                    Success = true,
                    Message = "Contract save successfully",
                    Data = contract,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ContractResponse>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ContractResponse>> SignAsync(Guid orderId, CancellationToken ct = default)
        {
            try
            {
                var contract = await _repo.GetTrackingByOrderIdAsync(orderId, ct);
                if (contract == null)
                    return new ApiResponse<ContractResponse> { Success = false, Message = "Contract not found" };

                if (contract.Status != ContractStatus.Draft)
                    return new ApiResponse<ContractResponse> { Success = false, Message = "Contract already signed" };

                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                    return new ApiResponse<ContractResponse> { Success = false, Message = "Order not found" };

                // chỉ cho ký nếu order đang ở trạng thái Processing
                if (order.Status != Repositories.Enum.Enum.OrderStatus.Processing)
                    return new ApiResponse<ContractResponse> { Success = false, Message = "Order must be paid before signing" };

                // buyer ký -> contract có hiệu lực
                contract.SignedAt = DateTime.UtcNow;
                contract.UpdatedAt = DateTime.UtcNow;
                contract.Status = ContractStatus.Active;
                await _repo.UpdateAsync(contract);

                return new ApiResponse<ContractResponse>
                {
                    Success = true,
                    Message = "Contract signed successfully",
                    Data = contract.Adapt<ContractResponse>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sign contract failed");
                return new ApiResponse<ContractResponse> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ContractResponse>> GetByOrderAsync(Guid orderId, CancellationToken ct = default)
        {
            var c = await _repo.GetByOrderIdAsync(orderId, ct);
            if (c == null) return new ApiResponse<ContractResponse> { Success = false, Message = "Contract not found" };
            return new ApiResponse<ContractResponse> { Success = true, Data = c.Adapt<ContractResponse>() };
        }

        public async Task<ApiResponse<ContractResponse>> CancelAsync(Guid orderId, string? reason, CancellationToken ct = default)
        {
            try
            {
                var c = await _repo.GetTrackingByOrderIdAsync(orderId, ct);
                if (c == null) return new ApiResponse<ContractResponse> { Success = false, Message = "Contract not found" };
                if (c.SignedAt != null) return new ApiResponse<ContractResponse> { Success = false, Message = "Contract already signed" };

                // nếu có cột status text: "cancelled"
                // nếu dùng enum + converter: gán enum tương ứng
                c.Status = ContractStatus.Cancelled;
                c.UpdatedAt = DateTime.UtcNow;
                // (tuỳ chọn) c.Status = "cancelled"; c.CancelReason = reason;
                await _repo.UpdateAsync(c);

                return new ApiResponse<ContractResponse> { Success = true, Message = "Contract cancelled", Data = c.Adapt<ContractResponse>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cancel contract failed");
                return new ApiResponse<ContractResponse> { Success = false, Message = ex.Message };
            }
        }
    }
}