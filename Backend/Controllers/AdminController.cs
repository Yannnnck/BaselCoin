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

    // 📌 1. Admin kann neue Benutzer erstellen
    [HttpPost("createUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Überprüfung, ob der Benutzername bereits existiert
        var existingUser = await _users.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
        if (existingUser != null)
            return BadRequest(new { message = "Benutzername bereits vergeben." });

        // Passwortvalidierung: Mindestanforderungen prüfen
        if (!PasswordHelper.IsValidPassword(request.Password))
            return BadRequest(new { message = "Passwort muss zwischen 15-64 Zeichen haben, eine Zahl, einen Groß- & Kleinbuchstaben sowie ein Sonderzeichen enthalten." });

        // Neuen Benutzer mit gehashtem Passwort anlegen
        var newUser = new User
        {
            Username = request.Username,
            HashedPassword = PasswordHelper.HashPassword(request.Password),
            Role = request.Role ?? "User" // Falls keine Rolle angegeben wird, Standard: "User"
        };

        await _users.InsertOneAsync(newUser);
        Logger.Log(User.Identity.Name, $"Neuen Benutzer erstellt: {request.Username} (Rolle: {newUser.Role})");

        return Ok(new { message = "Benutzer erfolgreich erstellt." });
    }

    // 📌 2. Admin kann Benutzer löschen
    [HttpDelete("deleteUser/{username}")]
    public async Task<IActionResult> DeleteUser(string username)
    {
        var result = await _users.DeleteOneAsync(u => u.Username == username);
        if (result.DeletedCount == 0)
            return NotFound(new { message = "Benutzer nicht gefunden." });

        Logger.Log(User.Identity.Name, $"Benutzer gelöscht: {username}");
        return Ok(new { message = "Benutzer wurde erfolgreich gelöscht." });
    }

    // 📌 3. Admin kann die Benutzerliste abrufen (Benutzername & Rolle)
    [HttpGet("getUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _users.Find(_ => true)
            .Project(u => new { u.Username, u.Role }) // Rolle auch mit ausgeben
            .ToListAsync();
        return Ok(users);
    }

    // 📌 4. Admin kann die Rolle eines Benutzers ändern
    [HttpPut("updateUserRole")]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
    {
        var update = Builders<User>.Update.Set(u => u.Role, request.NewRole);
        var result = await _users.UpdateOneAsync(u => u.Username == request.Username, update);

        if (result.MatchedCount == 0)
            return NotFound(new { message = "Benutzer nicht gefunden." });

        Logger.Log(User.Identity.Name, $"Rolle von {request.Username} geändert zu: {request.NewRole}");
        return Ok(new { message = "Benutzerrolle erfolgreich aktualisiert." });
    }
}

// 📌 Model für das Erstellen eines neuen Benutzers
public class CreateUserRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // Optional: Admin kann Benutzerrolle setzen
}

// 📌 Model für das Ändern der Benutzerrolle
public class UpdateUserRoleRequest
{
    public string Username { get; set; }
    public string NewRole { get; set; }
}