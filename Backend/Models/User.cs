using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

/// <summary>
/// Repräsentiert einen Benutzer in der Datenbank.
/// </summary>
public class User
{
    /// <summary>
    /// Eindeutige Benutzer-ID in MongoDB.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Eindeutiger Benutzername.
    /// </summary>
    [BsonElement("username")]
    [BsonRequired] // Feld ist erforderlich
    public string Username { get; set; }

    /// <summary>
    /// Gehashtes Passwort des Benutzers.
    /// </summary>
    [BsonElement("password")]
    [BsonRequired] // Feld ist erforderlich
    public string HashedPassword { get; set; }

    /// <summary>
    /// Rolle des Benutzers (z. B. "Admin" oder "User").
    /// </summary>
    [BsonElement("role")]
    [BsonDefaultValue("User")] // Standardrolle ist "User"
    public string Role { get; set; } = "User";

    /// <summary>
    /// (Optional) E-Mail-Adresse für Passwort-Zurücksetzen.
    /// </summary>
    [BsonElement("email")]
    [BsonIgnoreIfNull] // Wird nicht gespeichert, wenn null
    public string Email { get; set; }
}
