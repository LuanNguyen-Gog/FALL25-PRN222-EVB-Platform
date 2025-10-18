﻿using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System;
using System.Collections.Generic;
using static Repositories.Enum.Enum;

namespace Repositories.DBContext;

public partial class EVBatteryTradingContext : DbContext
{
    public EVBatteryTradingContext(DbContextOptions<EVBatteryTradingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Battery> Batteries { get; set; }

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("asset_status_enum", new[] { "available", "reserved", "sold", "archived" })
            .HasPostgresEnum("complaint_status_enum", new[] { "open", "in_progress", "resolved", "rejected" })
            .HasPostgresEnum("listing_status_enum", new[] { "draft", "pending", "active", "rejected", "sold", "archived" })
            .HasPostgresEnum("order_status_enum", new[] { "pending", "processing", "completed", "cancelled" })
            .HasPostgresEnum("payment_status_enum", new[] { "pending", "success", "failed", "refunded" })
            .HasPostgresEnum("user_status_enum", new[] { "active", "inactive" })
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Battery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("batteries_pkey");

            entity.ToTable("batteries");

            entity.HasIndex(e => e.Brand, "idx_batteries_brand");

            entity.HasIndex(e => e.OwnerId, "idx_batteries_owner");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BatteryCapacityKwh)
                .HasPrecision(7, 3)
                .HasColumnName("battery_capacity_kwh");
            entity.Property(e => e.BatteryHealthPct)
                .HasPrecision(5, 2)
                .HasColumnName("battery_health_pct");
            entity.Property(e => e.Brand)
                .IsRequired()
                .HasColumnName("brand");
            entity.Property(e => e.Chemistry).HasColumnName("chemistry");
            entity.Property(e => e.CompatibilityNote).HasColumnName("compatibility_note");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CycleCount).HasColumnName("cycle_count");
            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.NominalVoltageV)
                .HasPrecision(7, 2)
                .HasColumnName("nominal_voltage_v");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Owner).WithMany(p => p.Batteries)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("batteries_owner_id_fkey");
        });

        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("complaints_pkey");

            entity.ToTable("complaints");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnName("description");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("complaints_order_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("complaints_user_id_fkey");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contracts_pkey");

            entity.ToTable("contracts");

            entity.HasIndex(e => e.OrderId, "contracts_order_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContractFileUrl).HasColumnName("contract_file_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.SignedAt).HasColumnName("signed_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithOne(p => p.Contract)
                .HasForeignKey<Contract>(d => d.OrderId)
                .HasConstraintName("contracts_order_id_fkey");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("listings_pkey");

            entity.ToTable("listings");

            entity.HasIndex(e => e.PriceVnd, "idx_listings_price");

            entity.HasIndex(e => e.BatteryId, "uq_listings_battery_active")
                .IsUnique()
                .HasFilter("((battery_id IS NOT NULL) AND (status = ANY (ARRAY['pending'::listing_status_enum, 'active'::listing_status_enum])))");

            entity.HasIndex(e => e.VehicleId, "uq_listings_vehicle_active")
                .IsUnique()
                .HasFilter("((vehicle_id IS NOT NULL) AND (status = ANY (ARRAY['pending'::listing_status_enum, 'active'::listing_status_enum])))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AiSuggestedPriceVnd)
                .HasPrecision(14)
                .HasColumnName("ai_suggested_price_vnd");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.BatteryId).HasColumnName("battery_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PriceVnd)
                .HasPrecision(14)
                .HasColumnName("price_vnd");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.ListingApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("listings_approved_by_fkey");

            entity.HasOne(d => d.Battery).WithOne(p => p.Listing)
                .HasForeignKey<Listing>(d => d.BatteryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("listings_battery_id_fkey");

            entity.HasOne(d => d.Seller).WithMany(p => p.ListingSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("listings_seller_id_fkey");

            entity.HasOne(d => d.Vehicle).WithOne(p => p.Listing)
                .HasForeignKey<Listing>(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("listings_vehicle_id_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.HasIndex(e => e.BuyerId, "idx_orders_buyer");

            entity.HasIndex(e => e.ListingId, "orders_listing_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ListingId).HasColumnName("listing_id");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("now()")
                .HasColumnName("order_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Buyer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_buyer_id_fkey");

            entity.HasOne(d => d.Listing).WithOne(p => p.Order)
                .HasForeignKey<Order>(d => d.ListingId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_listing_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.HasIndex(e => e.OrderId, "payments_order_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AmountVnd)
                .HasPrecision(14)
                .HasColumnName("amount_vnd");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Method)
                .IsRequired()
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.ProviderTxnId).HasColumnName("provider_txn_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.OrderId)
                .HasConstraintName("payments_order_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.ExpiresAt, "idx_refresh_tokens_expires");

            entity.HasIndex(e => e.UserId, "idx_refresh_tokens_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.TokenHash)
                .IsRequired()
                .HasColumnName("token_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_user_id_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.ReviewerId, "idx_reviews_reviewer");

            entity.HasIndex(e => e.OrderId, "reviews_order_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");

            entity.HasOne(d => d.Order).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.OrderId)
                .HasConstraintName("reviews_order_id_fkey");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("reviews_reviewer_id_fkey");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("settings_pkey");

            entity.ToTable("settings");

            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasColumnName("value");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Settings)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("settings_updated_by_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Role, "idx_users_role");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => new { e.ExternalProvider, e.ExternalSubject }, "ux_users_provider_sub")
                .IsUnique()
                .HasFilter("((external_provider IS NOT NULL) AND (external_subject IS NOT NULL))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.EmailVerified).HasColumnName("email_verified");
            entity.Property(e => e.ExternalProvider).HasColumnName("external_provider");
            entity.Property(e => e.ExternalSubject).HasColumnName("external_subject");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.Role)
                .IsRequired()
                .HasDefaultValueSql("'member'::text")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vehicles_pkey");

            entity.ToTable("vehicles");

            entity.HasIndex(e => new { e.Brand, e.Model }, "idx_vehicles_brand_model");

            entity.HasIndex(e => e.OwnerId, "idx_vehicles_owner");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Brand)
                .IsRequired()
                .HasColumnName("brand");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Model)
                .IsRequired()
                .HasColumnName("model");
            entity.Property(e => e.OdometerKm).HasColumnName("odometer_km");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Owner).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("vehicles_owner_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // ----- ENUM → TEXT mapping -----
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLower(), // write to DB
                v => System.Enum.Parse<OrderStatus>(
                    char.ToUpper(v[0]) + v.Substring(1)) // read from DB
            ).HasDefaultValue(OrderStatus.Pending);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLower(),
                v => System.Enum.Parse<PaymentStatus>(
                    char.ToUpper(v[0]) + v.Substring(1))
            ).HasDefaultValue(PaymentStatus.Pending);

        modelBuilder.Entity<Complaint>()
            .Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v == ComplaintStatus.InProgress ? "in_progress" : v.ToString().ToLower(),
                v => v == "in_progress"
                    ? ComplaintStatus.InProgress
                    : System.Enum.Parse<ComplaintStatus>(
                        char.ToUpper(v[0]) + v.Substring(1))
            ).HasDefaultValue(ComplaintStatus.Open);

        modelBuilder.Entity<User>()
            .Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLower(),
                v => System.Enum.Parse<UserStatus>(
                    char.ToUpper(v[0]) + v.Substring(1))
            ).HasDefaultValue(UserStatus.Active);

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLower(),
                v => System.Enum.Parse<AssetStatus>(
                    char.ToUpper(v[0]) + v.Substring(1))
            ).HasDefaultValue(AssetStatus.Available);

        modelBuilder.Entity<Battery>()
            .Property(b => b.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLower(),
                v => System.Enum.Parse<AssetStatus>(
                    char.ToUpper(v[0]) + v.Substring(1))
            ).HasDefaultValue(AssetStatus.Available);

        modelBuilder.Entity<Listing>()
            .Property(l => l.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLower(),
                v => System.Enum.Parse<ListingStatus>(
                    char.ToUpper(v[0]) + v.Substring(1))
            ).HasDefaultValue(ListingStatus.Draft);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
