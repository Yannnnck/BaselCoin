using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
        var user = await _users.Find(u => u.Username == request.Username).FirstOrDefaultAsync();

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.HashedPassword))
        {
            Logger.Log(request.Username, "Fehlgeschlagener Login-Versuch", "WARNUNG");
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

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
