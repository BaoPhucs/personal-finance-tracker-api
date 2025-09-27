using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTrackerAPI.DTOs;
using PersonalFinanceTrackerAPI.Interfaces;
using PersonalFinanceTrackerAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PersonalFinanceTrackerAPI.Controllers
{
    
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class TransactionsController : ControllerBase
        {
            private readonly ITransactionRepository _txRepo;
            private readonly IUserRepository _userRepo;

            public TransactionsController(ITransactionRepository txRepo, IUserRepository userRepo)
            {
                _txRepo = txRepo;
                _userRepo = userRepo;
            }

        private Guid CurrentUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                      User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(sub))
                throw new UnauthorizedAccessException("Token không chứa user id");

            return Guid.Parse(sub);
        }


        [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var userId = CurrentUserId();
                var list = await _txRepo.GetByUserAsync(userId);
                var dtos = list.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Category = t.Category,
                    Type = t.Type,
                    Note = t.Note,
                    CreatedAt = t.Createdat.Value
                });

                return Ok(dtos);
            }

            [HttpGet("{id:guid}")]
            public async Task<IActionResult> Get(Guid id)
            {
                var tx = await _txRepo.GetByIdAsync(id);
                if (tx == null) return NotFound();
                if (tx.Userid != CurrentUserId()) return Forbid();

                var dto = new TransactionDto
                {
                    Id = tx.Id,
                    Amount = tx.Amount,
                    Category = tx.Category,
                    Type = tx.Type,
                    Note = tx.Note,
                    CreatedAt = tx.Createdat.Value
                };
                return Ok(dto);
            }

            [HttpPost]
            public async Task<IActionResult> Create(TransactionCreateDto dto)
            {
                var userId = CurrentUserId();
                var tx = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Userid = userId,
                    Amount = dto.Amount,
                    Category = dto.Category,
                    Type = dto.Type,
                    Note = dto.Note,
                    Createdat = DateTime.UtcNow
                };

                await _txRepo.AddAsync(tx);
                await _txRepo.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { id = tx.Id }, new TransactionDto { Id = tx.Id, Amount = tx.Amount, Category = tx.Category, Type = tx.Type, Note = tx.Note, CreatedAt = tx.Createdat.Value });
            }

            [HttpPut("{id:guid}")]
            public async Task<IActionResult> Update(Guid id, TransactionUpdateDto dto)
            {
                var tx = await _txRepo.GetByIdAsync(id);
                if (tx == null) return NotFound();
                if (tx.Userid != CurrentUserId()) return Forbid();

                tx.Amount = dto.Amount;
                tx.Category = dto.Category;
                tx.Type = dto.Type;
                tx.Note = dto.Note;

                await _txRepo.UpdateAsync(tx);
                await _txRepo.SaveChangesAsync();

                return NoContent();
            }

            [HttpDelete("{id:guid}")]
            public async Task<IActionResult> Delete(Guid id)
            {
                var tx = await _txRepo.GetByIdAsync(id);
                if (tx == null) return NotFound();
                if (tx.Userid != CurrentUserId()) return Forbid();

                await _txRepo.DeleteAsync(tx);
                await _txRepo.SaveChangesAsync();

                return NoContent();
            }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? category,
        [FromQuery] string? type,
        [FromQuery] string? sort = "desc"
    )
        {
            var userId = CurrentUserId();
            var results = await _txRepo.FilterTransactionsAsync(userId, startDate, endDate, category, type, sort);

            var dtos = results.Select(t => new TransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Category = t.Category,
                Type = t.Type,
                Note = t.Note,
                CreatedAt = t.Createdat.Value
            });

            return Ok(dtos);
        }
    }
}
