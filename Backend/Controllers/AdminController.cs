using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")] // Nur Admins haben Zugriff
public class AdminController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public AdminController(IMongoClient client)
    {
        var database = client.GetDatabase("BaselCoinDB");
        _users = database.GetCollection<User>("Users");
    }

    // 📌 2. Admin kann Benutzer anlegen
    [HttpPost("createUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
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
        Logger.Log(User.Identity.Name, $"Neuen Benutzer erstellt: {request.Username}");

        return Ok(new { message = "Benutzer erfolgreich erstellt" });
    }

    // 📌 3. Admin kann Benutzer löschen
    [HttpDelete("deleteUser/{username}")]
    public async Task<IActionResult> DeleteUser(string username)
    {
        var result = await _users.DeleteOneAsync(u => u.Username == username);
        if (result.DeletedCount == 0)
            return NotFound(new { message = "Benutzer nicht gefunden" });

        Logger.Log(User.Identity.Name, $"Benutzer gelöscht: {username}");
        return Ok(new { message = "Benutzer gelöscht" });
    }

    // 📌 4. Admin kann Kontostände abfragen
    [HttpGet("getUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _users.Find(_ => true).Project(u => new { u.Username }).ToListAsync();
        return Ok(users);
    }
}

public class CreateUserRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
