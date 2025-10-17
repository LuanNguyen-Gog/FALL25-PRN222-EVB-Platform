using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Battery
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public decimal? BatteryCapacityKwh { get; set; }

    public decimal? BatteryHealthPct { get; set; }

    public int? CycleCount { get; set; }

    public string Chemistry { get; set; }

    public decimal? NominalVoltageV { get; set; }

    public string CompatibilityNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Listing Listing { get; set; }

    public virtual User Owner { get; set; }
}
