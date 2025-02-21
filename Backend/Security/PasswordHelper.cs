using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Stellt Methoden zur sicheren Passwortverarbeitung bereit.
/// </summary>
public class PasswordHelper
{
    private const int SaltSize = 16; // Größe des Salt in Bytes
    private const int HashSize = 32; // Größe des Hash in Bytes
    private const int Iterations = 100000; // Anzahl der PBKDF2-Iterationen

    /// <summary>
    /// Überprüft, ob ein Passwort den Sicherheitsanforderungen entspricht.
    /// </summary>
    /// <param name="password">Das zu überprüfende Passwort.</param>
    /// <returns>Gibt true zurück, wenn das Passwort sicher ist, andernfalls false.</returns>
    public static bool IsValidPassword(string password)
    {
        return password.Length >= 15 && password.Length <= 64 &&
               Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$");
    }

    /// <summary>
    /// Erstellt einen sicheren Hash für ein Passwort mit PBKDF2.
    /// </summary>
    /// <param name="password">Das zu hashende Passwort.</param>
    /// <returns>Ein sicherer Hash mit Salt im Format: {Salt}:{Hash}</returns>
    public static string HashPassword(string password)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            // Salt generieren
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            // Passwort mit Salt hashen
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}"; // Salt und Hash als String speichern
            }
        }
    }

    /// <summary>
    /// Vergleicht ein Passwort mit einem gespeicherten Hash, um die Korrektheit zu überprüfen.
    /// </summary>
    /// <param name="password">Das eingegebene Passwort.</param>
    /// <param name="storedHash">Der gespeicherte Hash im Format {Salt}:{Hash}.</param>
    /// <returns>Gibt true zurück, wenn das Passwort korrekt ist, andernfalls false.</returns>
    public static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            // Salt und Hash extrahieren
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedPasswordHash = Convert.FromBase64String(parts[1]);

            // Hash des eingegebenen Passworts mit dem gespeicherten Salt berechnen
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);

                // Vergleichen, ob die Hashes übereinstimmen
                return CryptographicOperations.FixedTimeEquals(computedHash, storedPasswordHash);
            }
        }
        catch
        {
            return false;
        }
    }
}
