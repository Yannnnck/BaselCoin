using System;
using System.IO;

public static class Logger
{
    // Speicherort der Log-Datei
    private static readonly string logDirectory = "Logs";
    private static readonly string logFilePath = Path.Combine(logDirectory, "application.log");

    // Objekt für Thread-Synchronisation
    private static readonly object logLock = new object();

    /// <summary>
    /// Schreibt einen Log-Eintrag in die Datei und gibt ihn in der Konsole aus.
    /// </summary>
    /// <param name="user">Benutzername oder Systemprozess</param>
    /// <param name="action">Die durchgeführte Aktion</param>
    /// <param name="type">Log-Level (INFO, WARNUNG, FEHLER, etc.)</param>
    public static void Log(string user, string action, string type = "INFO")
    {
        try
        {
            // Falls das Verzeichnis nicht existiert, erstelle es
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            // Log-Eintrag formatieren
            string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {type} | {user} | {action}";

            // In Konsole ausgeben
            Console.WriteLine(logEntry);

            // Thread-Sicherheit gewährleisten und Log schreiben
            lock (logLock)
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            // Fehler beim Schreiben des Logs in die Konsole ausgeben
            Console.WriteLine($"FEHLER: Konnte Log nicht schreiben: {ex.Message}");
        }
    }
}
