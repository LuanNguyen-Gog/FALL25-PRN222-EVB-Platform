using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid BuyerId { get; set; }

    public Guid ListingId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Buyer { get; set; }

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual Contract Contract { get; set; }

    public virtual Listing Listing { get; set; }

    public virtual Payment Payment { get; set; }

    public virtual Review Review { get; set; }
}
