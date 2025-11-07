using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVBTradingContract.Response;

// DTO nhỏ gọn cho Order–Payment–Contract
public sealed record OrderMiniDto(Guid Id, string Status);

public sealed record PaymentMiniDto(
    Guid Id,
    string Status,
    string? ProviderTxnId,
    decimal? AmountVnd,
    DateTime? PaidAt
);

public sealed record ContractMiniDto(
    Guid Id,
    string Status,
    string? FileUrl,
    DateTime? SignedAt
);

// Gói tổng hợp trả về cho FE
public sealed record OrderAndContractResponse(
    OrderMiniDto Order,
    PaymentMiniDto? Payment,
    ContractMiniDto? Contract
);
