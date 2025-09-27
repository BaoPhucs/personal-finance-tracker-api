using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTrackerAPI.Data;
using PersonalFinanceTrackerAPI.Interfaces;
using PersonalFinanceTrackerAPI.Models;


namespace PersonalFinanceTrackerAPI.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;
        public TransactionRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Transaction tx)
        {
            await _db.Transactions.AddAsync(tx);
        }

        public async Task DeleteAsync(Transaction tx)
        {
            _db.Transactions.Remove(tx);
            await Task.CompletedTask;
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            return await _db.Transactions.FindAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetByUserAsync(Guid userId)
        {
            return await _db.Transactions.Where(t => t.Userid == userId)
                                         .OrderByDescending(t => t.Createdat)
                                         .ToListAsync();
        }

        public async Task UpdateAsync(Transaction tx)
        {
            _db.Transactions.Update(tx);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<(int Month, decimal Income, decimal Expense)>> GetMonthlySummaryAsync(Guid userId, int year)
        {
            var q = await _db.Transactions
                .Where(t => t.Userid == userId && t.Createdat.Value.Year == year)
                .GroupBy(t => t.Createdat.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Income = g.Where(x => x.Type == "Income").Sum(x => (decimal?)x.Amount) ?? 0,
                    Expense = g.Where(x => x.Type == "Expense").Sum(x => (decimal?)x.Amount) ?? 0
                })
                .ToListAsync();

            return q.Select(x => (x.Month, x.Income, x.Expense));
        }

        public async Task<IEnumerable<(string Category, decimal Total)>> GetCategorySummaryAsync(Guid userId, int year, string? type = null)
        {
            var q = _db.Transactions.Where(t => t.Userid == userId && t.Createdat.Value.Year == year);
            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(t => t.Type == type);

            var res = await q.GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(x => (decimal?)x.Amount) ?? 0 })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return res.Select(x => (x.Category, x.Total));
        }

        public async Task<IEnumerable<Transaction>> FilterTransactionsAsync(
            Guid userId,
            DateTime? startDate,
            DateTime? endDate,
            string? category,
            string? type,
            string? sort
        )
        {
            var query = _db.Transactions
                .Where(t => t.Userid == userId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.Createdat >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Createdat <= endDate.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category.ToLower().Contains(category.ToLower()));

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type == type);

            query = sort == "asc" ? query.OrderBy(t => t.Createdat) : query.OrderByDescending(t => t.Createdat);

            return await query.ToListAsync();
        }
    }
}
