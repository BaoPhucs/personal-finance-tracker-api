namespace PersonalFinanceTrackerAPI.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Passwordhash { get; set; } = null!;
    }
}
