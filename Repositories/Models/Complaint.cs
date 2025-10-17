using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Complaint
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? UserId { get; set; }

    public string Description { get; set; }
    public ComplaintStatus? Status { get; set; } // Enum

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; }

    public virtual User User { get; set; }
}
