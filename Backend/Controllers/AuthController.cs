using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public AuthController(IMongoClient client)
    {
        var database = client.GetDatabase("BaselCoinDB");
        _users = database.GetCollection<User>("Users");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (RateLimiter.IsBlocked(ip))
            return StatusCode(429, new { message = "Zu viele fehlgeschlagene Versuche. Bitte in 15 Minuten erneut versuchen." });

        var user = await _users.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        bool success = user != null && PasswordHelper.VerifyPassword(request.Password, user.HashedPassword);

        RateLimiter.RegisterAttempt(ip, success);

        if (!success)
        {
            Logger.Log(ip, $"Fehlgeschlagener Login für {request.Username}", "WARNUNG");
            return Unauthorized(new { message = "Ungültige Anmeldedaten" });
        }

        string token = TokenService.GenerateJwtToken(user.Username, user.Role);
        Logger.Log(user.Username, "Erfolgreich eingeloggt");

        return Ok(new { token, role = user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var username = User.Identity.Name;
        Logger.Log(username, "Abgemeldet");
        return Ok(new { message = "Logout erfolgreich" });
    }
}
public static class TokenService
{
    private static readonly string SecretKey = "DeinGeheimerSchluessel123456"; // Sicher aufbewahren!

    public static string GenerateJwtToken(string username, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(30), // Absolutes Timeout
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public static class RateLimiter
{
    private static ConcurrentDictionary<string, (int attempts, DateTime lastAttempt)> loginAttempts =
        new ConcurrentDictionary<string, (int, DateTime)>();

    public static bool IsBlocked(string ip)
    {
        if (loginAttempts.TryGetValue(ip, out var attempt))
        {
            if (attempt.attempts >= 5 && (DateTime.UtcNow - attempt.lastAttempt).TotalMinutes < 15)
                return true;
        }
        return false;
    }

    public static void RegisterAttempt(string ip, bool success)
    {
        if (success)
        {
            loginAttempts.TryRemove(ip, out _);
        }
        else
        {
            loginAttempts.AddOrUpdate(ip, (1, DateTime.UtcNow), (key, old) => (old.attempts + 1, DateTime.UtcNow));
        }
    }
}


public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    var existingUser = await _users.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
    if (existingUser != null)
        return BadRequest(new { message = "Benutzername bereits vergeben" });

    var newUser = new User
    {
        Username = request.Username,
        HashedPassword = PasswordHelper.HashPassword(request.Password),
        Role = "User"
    };

    await _users.InsertOneAsync(newUser);
    Logger.Log(request.Username, "Neuer Benutzer registriert");

    return Ok(new { message = "Registrierung erfolgreich!" });
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
