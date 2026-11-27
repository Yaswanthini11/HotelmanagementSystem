
using HotelBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data
{
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
    }
}
