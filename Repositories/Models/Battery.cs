using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Battery
{
    public long BatteryId { get; set; }

    public long OwnerId { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public decimal? BatteryCapacityKwh { get; set; }

    public decimal? BatteryHealthPct { get; set; }

    public int? CycleCount { get; set; }

    public string Chemistry { get; set; }

    public decimal? NominalVoltageV { get; set; }

    public string CompatibilityNote { get; set; }
    public AssetStatus? Status { get; set; } // Enum

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Listing Listing { get; set; }

    public virtual User Owner { get; set; }
}
