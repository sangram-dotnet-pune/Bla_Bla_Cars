using Microsoft.EntityFrameworkCore;

namespace UserService.Models
{
    public class AppUserDbContext(DbContextOptions<AppUserDbContext>options):DbContext(options)
    {


        public DbSet<AppUser>AppUsers { get; set; }
    }
}
