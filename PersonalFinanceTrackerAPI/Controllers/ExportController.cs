using ClosedXML.Excel;
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
    public class ExportController : ControllerBase
    {
        private readonly ITransactionRepository _txRepo;
        public ExportController(ITransactionRepository txRepo) => _txRepo = txRepo;

        private Guid CurrentUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(sub);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> ExportTransactions([FromQuery] int? year = null)
        {
            var userId = CurrentUserId();
            var transactions = (await _txRepo.GetByUserAsync(userId))
                                    .Where(t => !year.HasValue || t.Createdat.Value.Year == year.Value)
                                    .OrderByDescending(t => t.Createdat)
                                    .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Transactions");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Amount";
            ws.Cell(1, 3).Value = "Type";
            ws.Cell(1, 4).Value = "Category";
            ws.Cell(1, 5).Value = "Note";
            ws.Cell(1, 6).Value = "CreatedAt";

            for (int i = 0; i < transactions.Count; i++)
            {
                var r = i + 2;
                var t = transactions[i];
                ws.Cell(r, 1).Value = t.Id.ToString();
                ws.Cell(r, 2).Value = t.Amount;
                ws.Cell(r, 3).Value = t.Type;
                ws.Cell(r, 4).Value = t.Category;
                ws.Cell(r, 5).Value = t.Note ?? string.Empty;
                ws.Cell(r, 6).Value = t.Createdat.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var fileName = $"transactions_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
