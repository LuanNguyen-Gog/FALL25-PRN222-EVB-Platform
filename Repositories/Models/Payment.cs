using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Payment
{
    public long PaymentId { get; set; }

    public long OrderId { get; set; }

    public decimal AmountVnd { get; set; }

    public string Method { get; set; }
    public PaymentStatus EntityStatus { get; set; }  // Enum

    public DateTime? PaidAt { get; set; }

    public string ProviderTxnId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; }
}
