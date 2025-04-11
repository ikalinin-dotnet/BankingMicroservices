using System.ComponentModel.DataAnnotations;

namespace AccountService.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User"; // Default role
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}