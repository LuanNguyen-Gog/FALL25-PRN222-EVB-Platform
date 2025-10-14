using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Listing
{
    public long ListingId { get; set; }

    public long SellerId { get; set; }

    public long? VehicleId { get; set; }

    public long? BatteryId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public decimal? PriceVnd { get; set; }

    public decimal? AiSuggestedPriceVnd { get; set; }

    public long? ApprovedBy { get; set; }
    public ListingStatus EntityStatus { get; set; } // Enum

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User ApprovedByNavigation { get; set; }

    public virtual Battery Battery { get; set; }

    public virtual Order Order { get; set; }

    public virtual User Seller { get; set; }

    public virtual Vehicle Vehicle { get; set; }
}
