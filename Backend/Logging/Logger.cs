using System;
using System.IO;

public static class Logger
{
    private static readonly string logFilePath = "Logs/application.log";

    public static void Log(string user, string action, string type = "INFO")
    {
        string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {type} | {user} | {action}";
        Console.WriteLine(logEntry);

        lock (logFilePath)
        {
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
