namespace PersonalFinanceTracker.Api.Models;

public enum TransactionType
{
    Income,
    Expense
}

public sealed class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string Title { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public required string Category { get; set; }
    public DateTime Date { get; set; }
}
