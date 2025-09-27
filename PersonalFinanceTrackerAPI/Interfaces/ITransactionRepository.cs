using PersonalFinanceTrackerAPI.Models;


namespace PersonalFinanceTrackerAPI.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetByUserAsync(Guid userId);
        Task AddAsync(Transaction tx);
        Task UpdateAsync(Transaction tx);
        Task DeleteAsync(Transaction tx);
        Task SaveChangesAsync();

        // Aggregations
        Task<IEnumerable<(int Month, decimal Income, decimal Expense)>> GetMonthlySummaryAsync(Guid userId, int year);
        Task<IEnumerable<(string Category, decimal Total)>> GetCategorySummaryAsync(Guid userId, int year, string? type = null);

        Task<IEnumerable<Transaction>> FilterTransactionsAsync(
            Guid userId,
            DateTime? startDate,
            DateTime? endDate,
            string? category,
            string? type,
            string? sort
        );
    }
}
