using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class Vehicle
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public int? Year { get; set; }

    public int? OdometerKm { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Listing Listing { get; set; }

    public virtual User Owner { get; set; }
}
