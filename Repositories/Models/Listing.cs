using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Listing
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public Guid? VehicleId { get; set; }

    public Guid? BatteryId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public decimal? PriceVnd { get; set; }

    public Guid? ApprovedBy { get; set; }

    public ListingStatus? Status { get; set; } // Enum

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User ApprovedByNavigation { get; set; }

    public virtual Battery Battery { get; set; }

    public virtual Order Order { get; set; }

    public virtual User Seller { get; set; }

    public virtual Vehicle Vehicle { get; set; }
}
