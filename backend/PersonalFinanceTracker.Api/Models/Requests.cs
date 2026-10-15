namespace PersonalFinanceTracker.Api.Models;

public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record TransactionRequest(
    string Title,
    decimal Amount,
    TransactionType Type,
    string Category,
    DateTime Date);

public sealed record MoneyHabitsRequest(
    string TrackingFrequency,
    string Budgeting,
    string EmergencySavings,
    string ReviewFrequency);

public sealed record MoneyHabitRecommendation(
    string Area,
    string Title,
    string Message);

public sealed record MoneyHabitsResponse(
    int Score,
    string Level,
    string Summary,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<MoneyHabitRecommendation> Recommendations);
