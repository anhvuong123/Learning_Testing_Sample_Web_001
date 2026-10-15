using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Api.Models;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(JsonFileStore store, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email)
            || request.Password.Length < 6)
            return BadRequest(new { message = "Name, email, and a password of at least 6 characters are required." });

        var users = await store.ReadAsync<User>("users.json");
        var email = request.Email.Trim().ToLowerInvariant();
        if (users.Any(user => user.Email == email))
            return Conflict(new { message = "An account with that email already exists." });

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = HashPassword(request.Password)
        };
        users.Add(user);
        await store.WriteAsync("users.json", users);
        return Ok(new { token = tokens.CreateToken(user), user = new { user.Id, user.Name, user.Email } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = (await store.ReadAsync<User>("users.json"))
            .FirstOrDefault(candidate => candidate.Email == email);
        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new { token = tokens.CreateToken(user), user = new { user.Id, user.Name, user.Email } });
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string saved)
    {
        var parts = saved.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
