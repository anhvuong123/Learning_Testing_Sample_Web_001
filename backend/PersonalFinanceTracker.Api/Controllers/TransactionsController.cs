using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Models;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TransactionsController(JsonFileStore store) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> Get(
        [FromQuery] TransactionType? type,
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var userId = CurrentUserId();
        var filteredTransactions = (await store.ReadAsync<Transaction>("transactions.json"))
            .Where(item => item.UserId == userId)
            .Where(item => type is null || item.Type == type)
            .Where(item => string.IsNullOrWhiteSpace(category)
                || item.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(search)
                || item.Title.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Date)
            .ThenByDescending(item => item.Id)
            .ToList();

        var transactions = filteredTransactions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Ok(new
        {
            items = transactions,
            totalCount = filteredTransactions.Count,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(filteredTransactions.Count / (double)pageSize),
            totalIncome = filteredTransactions
                .Where(item => item.Type == TransactionType.Income)
                .Sum(item => item.Amount),
            totalExpenses = filteredTransactions
                .Where(item => item.Type == TransactionType.Expense)
                .Sum(item => item.Amount)
        });
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> Create(TransactionRequest request)
    {
        if (!IsValid(request)) return BadRequest(new { message = "All fields are required and amount must be greater than zero." });
        var transactions = await store.ReadAsync<Transaction>("transactions.json");
        var transaction = ToTransaction(request, CurrentUserId());
        transactions.Add(transaction);
        await store.WriteAsync("transactions.json", transactions);
        return CreatedAtAction(nameof(Get), new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Transaction>> Update(Guid id, TransactionRequest request)
    {
        if (!IsValid(request)) return BadRequest(new { message = "All fields are required and amount must be greater than zero." });
        var transactions = await store.ReadAsync<Transaction>("transactions.json");
        var transaction = transactions.FirstOrDefault(item => item.Id == id && item.UserId == CurrentUserId());
        if (transaction is null) return NotFound(new { message = "Transaction not found." });
        transaction.Title = request.Title.Trim();
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Category = request.Category.Trim();
        transaction.Date = request.Date;
        await store.WriteAsync("transactions.json", transactions);
        return Ok(transaction);
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static bool IsValid(TransactionRequest request) =>
        !string.IsNullOrWhiteSpace(request.Title) && request.Amount > 0 && !string.IsNullOrWhiteSpace(request.Category);
    private static Transaction ToTransaction(TransactionRequest request, Guid userId) => new()
    {
        UserId = userId, Title = request.Title.Trim(), Amount = request.Amount,
        Type = request.Type, Category = request.Category.Trim(), Date = request.Date
    };
}
