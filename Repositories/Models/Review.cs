using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Review
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ReviewerId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; }

    public virtual User Reviewer { get; set; }
}
