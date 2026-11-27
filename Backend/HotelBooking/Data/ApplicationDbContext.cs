
using HotelBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Hotel Management
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<HotelImage> HotelImages { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<HotelAmenity> HotelAmenities { get; set; }

    // Room Management
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<RoomTypeAmenity> RoomTypeAmenities { get; set; }
    public DbSet<RoomInventory> RoomInventories { get; set; }

    // Booking Management
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingItem> BookingItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Hotel and Amenity - Composite Primary Key
        builder.Entity<HotelAmenity>()
            .HasKey(ha => new { ha.HotelId, ha.AmenityId });

        // Room Type and Amenity - Composite Primary Key
        builder.Entity<RoomTypeAmenity>()
            .HasKey(rta => new { rta.RoomTypeId, rta.AmenityId });

        // Room Inventory - Primary Key
        builder.Entity<RoomInventory>()
            .HasKey(ri => ri.InventoryId);

        // Booking and BookingItem relationship
        builder.Entity<BookingItem>()
            .HasOne(bi => bi.Booking)
            .WithMany(b => b.BookingItems)
            .HasForeignKey(bi => bi.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // RoomType and BookingItem relationship
        builder.Entity<BookingItem>()
            .HasOne(bi => bi.RoomType)
            .WithMany(rt => rt.BookingItems)
            .HasForeignKey(bi => bi.RoomTypeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
