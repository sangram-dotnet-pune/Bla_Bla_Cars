using Microsoft.EntityFrameworkCore;

namespace TripService.Models
{
    public class TripDbContext(DbContextOptions<TripDbContext> options):DbContext(options)
    {


        public DbSet<Trip> Trips { get; set; }
    }
}
