using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Complaint
{
    public long ComplaintId { get; set; }

    public long? OrderId { get; set; }

    public long? UserId { get; set; }

    public string Description { get; set; }
    public ComplaintStatus EntityStatus { get; set; } // Enum

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; }

    public virtual User User { get; set; }
}
