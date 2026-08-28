using Microsoft.EntityFrameworkCore;

namespace UserService.Models
{
    public class AppUserDbContext(DbContextOptions<AppUserDbContext>options):DbContext(options)
    {


        public DbSet<AppUser>AppUsers { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(review => review.Id);
                entity.Property(review => review.Comment).HasMaxLength(1000);
                entity.Property(review => review.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(review => review.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(review => new
                    { review.BookingId, review.ReviewerId, review.RevieweeId })
                    .IsUnique();

                entity.HasOne(review => review.Reviewer)
                    .WithMany(user => user.ReviewsWritten)
                    .HasForeignKey(review => review.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(review => review.Reviewee)
                    .WithMany(user => user.ReviewsReceived)
                    .HasForeignKey(review => review.RevieweeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
