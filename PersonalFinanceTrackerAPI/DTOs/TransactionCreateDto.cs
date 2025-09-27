namespace PersonalFinanceTrackerAPI.DTOs
{
    public class TransactionCreateDto
    {
        public decimal Amount { get; set; }
        public string Category { get; set; } = null!;
        public string Type { get; set; } = null!; // "Income" or "Expense"
        public string? Note { get; set; }
    }
}
