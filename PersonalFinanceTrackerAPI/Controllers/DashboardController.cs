using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTrackerAPI.Interfaces;
using PersonalFinanceTrackerAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PersonalFinanceTrackerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ITransactionRepository _txRepo;
        public DashboardController(ITransactionRepository txRepo)
        {
            _txRepo = txRepo;
        }

        private Guid CurrentUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(sub);
        }

        // GET api/dashboard/monthly?year=2025
        [HttpGet("monthly")]
        public async Task<IActionResult> Monthly([FromQuery] int year)
        {
            var userId = CurrentUserId();
            var summary = await _txRepo.GetMonthlySummaryAsync(userId, year);

            // return 12 months fill 0 if missing
            var months = Enumerable.Range(1, 12)
                .Select(m =>
                {
                    var item = summary.FirstOrDefault(x => x.Month == m);
                    return new { Month = m, Income = item.Income, Expense = item.Expense };
                });

            return Ok(months);
        }

        // GET api/dashboard/categories?year=2025&type=Expense
        [HttpGet("categories")]
        public async Task<IActionResult> Categories([FromQuery] int year, [FromQuery] string? type = null)
        {
            var userId = CurrentUserId();
            var cats = await _txRepo.GetCategorySummaryAsync(userId, year, type);
            var res = cats.Select(x => new { Category = x.Category, Total = x.Total });
            return Ok(res);
        }
    }
}
