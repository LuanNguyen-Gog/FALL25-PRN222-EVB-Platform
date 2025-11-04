using EVBTradingContract.Common;
using EVBTradingContract.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IContractService
    {
        Task<ApiResponse<ContractResponse>> UpsertForOrderAsync(Guid orderId, string fileUrl, CancellationToken ct = default);
        Task<ApiResponse<ContractResponse>> SignAsync(Guid orderId, CancellationToken ct = default);
        Task<ApiResponse<ContractResponse>> GetByOrderAsync(Guid orderId, CancellationToken ct = default);
    }
}
