using Microsoft.EntityFrameworkCore;

namespace TripService.Models
{
    public class TripDbContext(DbContextOptions<TripDbContext> options):DbContext(options)
    {


        public DbSet<Trip> Trips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trip>()
                .Property(trip => trip.PricePerSeat)
                .HasPrecision(10, 2);
        }
    }
}
