using Microsoft.EntityFrameworkCore;

namespace BookingService.Models
{
    public class BookingDbContext(DbContextOptions<BookingDbContext> options):DbContext(options)
    {
        public DbSet<Booking> Bookings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.BookingId);

                entity.Property(b => b.TotalAmount)
                      .HasColumnType("decimal(10,2)");

                entity.Property(b => b.Status)
                      .HasConversion<string>();  

                entity.Property(b => b.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
