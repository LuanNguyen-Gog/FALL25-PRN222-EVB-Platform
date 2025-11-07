using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Contract
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string ContractFileUrl { get; set; }

    public ContractStatus? Status { get; set; }

    public DateTime? SignedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; }
}
