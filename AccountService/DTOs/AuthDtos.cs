namespace AccountService.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public int? CustomerId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}