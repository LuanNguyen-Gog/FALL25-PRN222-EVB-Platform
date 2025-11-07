using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid BuyerId { get; set; }
    public Guid? BatteryId { get; set; }
    public Guid? VehicleId { get; set; }

    public Guid? ListingId { get; set; }

    public DateTime OrderDate { get; set; }
    public OrderStatus? Status { get; set; } // Enum

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Buyer { get; set; }

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public virtual Battery Battery { get; set; }
    public virtual Vehicle Vehicle { get; set; }

    public virtual Contract Contract { get; set; }

    public virtual Listing? Listing { get; set; }

    public virtual Payment Payment { get; set; }

    public virtual Review Review { get; set; }
}
