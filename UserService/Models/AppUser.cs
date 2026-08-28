using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class AppUser
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string? MiniBio { get; set; }
        public string? TravelPreferences { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }
        public int? VehicleYear { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehicleLicensePlate { get; set; }

        [Required]
        public string Password { get; set; }

        public ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();
        public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    }
}
