using LaGranjaApi.Data;
using LaGranjaApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración del puerto de Render / Local
var renderPort = Environment.GetEnvironmentVariable("PORT")
    ?? Environment.GetEnvironmentVariable("Port")
    ?? "10000";

builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");

// 1. Base de Datos Local SQLite (Modo Resiliente / Local)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=LaGranjaLocal.db";

builder.Services.AddDbContext<LaGranjaDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Servicio Firebase Cloud Firestore (Modo Híbrido Dual)
builder.Services.AddSingleton<FirebaseService>();

// 3. CORS abierto para Frontend local, Firebase Hosting, Vercel o Render
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar SQLite y sembrar catálogo inicial al arrancar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LaGranjaDbContext>();
    db.Database.EnsureCreated();
    db.SeedDataIfEmpty();
}

app.UseCors();

if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoint de verificación de salud e infraestructura (Render Health Check & Rúbrica)
app.MapGet("/health", (FirebaseService firebase) => Results.Ok(new
{
    status = "ok",
    api = "Brostería La Granja API",
    version = "1.0-Multiplataforma",
    baseDeDatosLocal = "SQLite (LaGranjaLocal.db) Activa",
    baseDeDatosNube = firebase.IsAvailable
        ? $"Firebase Firestore Conectado (Proyecto: {firebase.ProjectId})"
        : "Firebase en Modo Dual Resiliente (Fallback a SQLite activo)"
}));

// Endpoint raíz informativo
app.MapGet("/", (FirebaseService firebase) => Results.Ok(new
{
    mensaje = "API Brostería La Granja funcionando correctamente",
    serviciosDisponibles = new[]
    {
        "/api/auth/login",
        "/api/auth/register",
        "/api/categorias",
        "/api/productos",
        "/api/promociones",
        "/api/pedidos",
        "/api/resenas",
        "/api/reservas",
        "/health"
    },
    infraestructura = new
    {
        dbLocal = "SQLite (LaGranjaLocal.db)",
        dbNube = firebase.IsAvailable ? "Firebase Firestore Activo" : "Firebase Modo Dual / Local",
        entorno = app.Environment.EnvironmentName
    }
}));

app.MapControllers();

app.Run();
