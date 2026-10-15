using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Models;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/public")]
public sealed class PublicController : ControllerBase
{
    [HttpPost("money-habits")]
    public ActionResult<MoneyHabitsResponse> CheckMoneyHabits(MoneyHabitsRequest request)
    {
        if (!IsValid(request))
            return BadRequest(new { message = "Please answer all money habits questions." });

        var score = Score(request);
        var recommendations = new List<MoneyHabitRecommendation>();
        var strengths = new List<string>();

        AddTrackingFeedback(request.TrackingFrequency, strengths, recommendations);
        AddBudgetingFeedback(request.Budgeting, strengths, recommendations);
        AddSavingsFeedback(request.EmergencySavings, strengths, recommendations);
        AddReviewFeedback(request.ReviewFrequency, strengths, recommendations);

        var level = score switch
        {
            >= 80 => "Strong foundation",
            >= 55 => "Getting started",
            _ => "Start with one small step"
        };
        var summary = score switch
        {
            >= 80 => "Your money habits are already consistent. Keep your routine and improve one detail at a time.",
            >= 55 => "You have a useful foundation. A few consistent habits can make your financial picture clearer.",
            _ => "You do not need to change everything at once. Start with one small habit and build from there."
        };

        return Ok(new MoneyHabitsResponse(score, level, summary, strengths, recommendations));
    }

    private static bool IsValid(MoneyHabitsRequest request) =>
        new[]
        {
            request.TrackingFrequency, request.Budgeting,
            request.EmergencySavings, request.ReviewFrequency
        }.All(value => !string.IsNullOrWhiteSpace(value));

    private static int Score(MoneyHabitsRequest request) =>
        TrackingScore(request.TrackingFrequency)
        + BudgetingScore(request.Budgeting)
        + SavingsScore(request.EmergencySavings)
        + ReviewScore(request.ReviewFrequency);

    private static int TrackingScore(string value) => value switch
    {
        "Daily" => 25,
        "Weekly" => 20,
        "Rarely" => 10,
        _ => 0
    };

    private static int BudgetingScore(string value) => value switch
    {
        "Yes" => 25,
        "Sometimes" => 17,
        "No" => 8,
        _ => 0
    };

    private static int SavingsScore(string value) => value switch
    {
        "Yes" => 25,
        "I am starting" => 17,
        "Not yet" => 8,
        _ => 0
    };

    private static int ReviewScore(string value) => value switch
    {
        "Weekly" => 25,
        "Monthly" => 18,
        "Rarely" => 8,
        _ => 0
    };

    private static void AddTrackingFeedback(string value, List<string> strengths, List<MoneyHabitRecommendation> recommendations)
    {
        if (value is "Daily" or "Weekly")
            strengths.Add("You already have a regular spending-tracking habit.");
        if (value == "Daily")
            recommendations.Add(new("Tracking", "Keep your routine sustainable", "A short weekly review can help you turn daily entries into useful patterns."));
        else if (value == "Weekly")
            recommendations.Add(new("Tracking", "Make tracking more consistent", "Record expenses soon after each purchase so small costs do not get missed."));
        else
            recommendations.Add(new("Tracking", "Start with one weekly check-in", "Record your main expenses once a week. A simple routine is better than trying to track everything perfectly."));
    }

    private static void AddBudgetingFeedback(string value, List<string> strengths, List<MoneyHabitRecommendation> recommendations)
    {
        if (value == "Yes")
            strengths.Add("You use a monthly budget to plan your money.");
        if (value == "Yes")
            recommendations.Add(new("Budgeting", "Compare plan and reality", "At the end of each month, compare planned amounts with your actual spending."));
        else if (value == "Sometimes")
            recommendations.Add(new("Budgeting", "Try a simple monthly plan", "Start with three groups: needs, wants, and savings. You can add detail later."));
        else
            recommendations.Add(new("Budgeting", "Create your first simple budget", "Begin with your monthly income and three essential expense categories. Keep it easy to maintain."));
    }

    private static void AddSavingsFeedback(string value, List<string> strengths, List<MoneyHabitRecommendation> recommendations)
    {
        if (value == "Yes")
            strengths.Add("You have started building an emergency savings buffer.");
        if (value == "Yes")
            recommendations.Add(new("Savings", "Review your safety buffer", "Check that your emergency savings still match your essential monthly expenses."));
        else if (value == "I am starting")
            recommendations.Add(new("Savings", "Make saving automatic", "Set a small fixed amount aside each month to build consistency before increasing the target."));
        else
            recommendations.Add(new("Savings", "Start with a small buffer", "The first goal is building the habit. Even a modest amount can be a useful first step."));
    }

    private static void AddReviewFeedback(string value, List<string> strengths, List<MoneyHabitRecommendation> recommendations)
    {
        if (value == "Weekly")
            strengths.Add("You review your spending regularly.");
        if (value == "Weekly")
            recommendations.Add(new("Review", "Look for patterns", "Use your weekly review to notice repeated expenses and unusual transactions."));
        else if (value == "Monthly")
            recommendations.Add(new("Review", "Add a short weekly check", "Keep your monthly review, then add ten minutes each week to catch issues earlier."));
        else
            recommendations.Add(new("Review", "Review your three largest expenses", "Set aside ten minutes each week to check recent transactions and the biggest costs."));
    }
}
