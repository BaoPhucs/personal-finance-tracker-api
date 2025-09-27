namespace PersonalFinanceTrackerAPI.DTOs
{
    public class TransactionUpdateDto
    {
        public decimal Amount { get; set; }
        public string Category { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Note { get; set; }
    }
}
