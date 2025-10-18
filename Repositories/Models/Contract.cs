using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Contract
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string ContractFileUrl { get; set; }

    public DateTime? SignedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; }
}
