using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public decimal AmountVnd { get; set; }

    public string Method { get; set; }

    public DateTime? PaidAt { get; set; }

    public string ProviderTxnId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; }
}
