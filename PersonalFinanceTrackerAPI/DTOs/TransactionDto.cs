namespace PersonalFinanceTrackerAPI.DTOs
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
