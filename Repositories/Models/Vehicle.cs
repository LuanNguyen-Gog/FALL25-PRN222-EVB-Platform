using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class Vehicle
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public int? Year { get; set; }

    public int? OdometerKm { get; set; }
    public AssetStatus? Status { get; set; } // Enum

    public decimal PriceVnd { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Listing> Listing { get; set; } = new List<Listing>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User Owner { get; set; }
}
