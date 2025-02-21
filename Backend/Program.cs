using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 📌 Konfigurationsdateien laden (appsettings.json + Umgebungsvariablen)
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// 📌 MongoDB Verbindung herstellen
var mongoConnectionString = configuration["MONGODB_URI"] ?? "mongodb://localhost:27017/baselcoin";
var mongoClient = new MongoClient(mongoConnectionString);
builder.Services.AddSingleton<IMongoClient>(mongoClient);

// 📌 JWT-Authentifizierung konfigurieren
var jwtKey = Encoding.ASCII.GetBytes(configuration["JWT_SECRET"] ?? "FallbackGeheimerSchluessel");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true; // HTTPS erforderlich für JWT
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero // Sofortige Token-Ablaufprüfung
    };
});

// 📌 CORS (Cross-Origin Resource Sharing) aktivieren, falls API von extern aufgerufen wird
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://meinfrontend.de") // Erlaubte Domains anpassen
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 📌 Logging aktivieren
builder.Services.AddLogging();

// 📌 Authorization-Dienst hinzufügen
builder.Services.AddAuthorization();

var app = builder.Build();

// 📌 Sicherheits-Header setzen
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'");
    await next();
});

// 📌 Middleware-Pipeline konfigurieren
app.UseHttpsRedirection(); // HTTP auf HTTPS umleiten
app.UseHsts(); // HTTP Strict Transport Security aktivieren

app.UseCors("AllowSpecificOrigins"); // CORS aktivieren

app.UseAuthentication(); // JWT-Authentifizierung aktivieren
app.UseAuthorization(); // Autorisierungsmechanismus aktivieren

app.MapControllers(); // Automatische API-Routenregistrierung

app.Run();