namespace PersonalFinanceTrackerAPI.DTOs
{
    public class LoginDto
    {
        public string UsernameOrEmail { get; set; } = null!;
        public string Passwordhash { get; set; } = null!;
    }
}
