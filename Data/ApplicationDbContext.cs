using DesertPaths.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DesertPaths.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Land> Lands => Set<Land>();
    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<JourneyStyle> JourneyStyles => Set<JourneyStyle>();
    public DbSet<JourneyItinerary> JourneyItineraries => Set<JourneyItinerary>();
    public DbSet<JourneyImage> JourneyImages => Set<JourneyImage>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable legacy timestamp behavior for PostgreSQL
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Land configuration
        builder.Entity<Land>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Journey configuration
        builder.Entity<Journey>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.Land)
                .WithMany(l => l.Journeys)
                .HasForeignKey(e => e.LandId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DefaultStyle)
                .WithMany(s => s.Journeys)
                .HasForeignKey(e => e.DefaultStyleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JourneyItinerary configuration
        builder.Entity<JourneyItinerary>(entity =>
        {
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Itineraries)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JourneyImage configuration
        builder.Entity<JourneyImage>(entity =>
        {
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Images)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking configuration
        builder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => e.BookingReference).IsUnique();

            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Bookings)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Style)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.StyleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment configuration (one booking can have multiple payment attempts)
        builder.Entity<Payment>(entity =>
        {
            entity.HasOne(e => e.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review configuration
        builder.Entity<Review>(entity =>
        {
            entity.HasOne(e => e.Journey)
                .WithMany(j => j.Reviews)
                .HasForeignKey(e => e.JourneyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
