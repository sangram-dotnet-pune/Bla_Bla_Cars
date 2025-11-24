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


        [Required]
        public string Password { get; set; }
    }
}
