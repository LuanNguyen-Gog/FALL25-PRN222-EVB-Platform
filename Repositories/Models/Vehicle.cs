using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Vehicle
{
    public long VehicleId { get; set; }

    public long OwnerId { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public int? Year { get; set; }

    public int? OdometerKm { get; set; }
    public AssetStatus? Status { get; set; } // Enum

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Listing Listing { get; set; }

    public virtual User Owner { get; set; }
}
