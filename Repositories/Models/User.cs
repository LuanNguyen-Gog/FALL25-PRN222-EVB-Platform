using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string Phone { get; set; }

    public string Role { get; set; }
    public UserStatus? Status { get; set; } // Enum
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual ICollection<Listing> ListingApprovedByNavigations { get; set; } = new List<Listing>();

    public virtual ICollection<Listing> ListingSellers { get; set; } = new List<Listing>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Setting> Settings { get; set; } = new List<Setting>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
