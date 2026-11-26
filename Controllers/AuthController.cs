using LaGranjaApi.Data;
using LaGranjaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LaGranjaDbContext _db;
    private readonly ILogger<AuthController> _logger;

    public AuthController(LaGranjaDbContext db, ILogger<AuthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static string BuildToken(Usuario usuario) =>
        Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "." +
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{usuario.Id}:{usuario.Email}:{usuario.Rol}"));

    /// <summary>
    /// Inicio de sesión para clientes y administrador de Brostería La Granja
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResponse(
                false,
                "El correo y la contraseña son obligatorios.",
                "", "", "", "", "", "", 0
            ));
        }

        var emailNorm = request.Email.Trim().ToLower();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm);

        if (usuario == null || !LaGranjaDbContext.VerifyPassword(request.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("Intento de acceso fallido para: {Email}", request.Email);
            return Unauthorized(new AuthResponse(
                false,
                "Correo o contraseña incorrectos.",
                "", "", "", "", "", "", 0
            ));
        }

        if (!usuario.Activo)
        {
            return StatusCode(403, new AuthResponse(
                false,
                "Esta cuenta ha sido deshabilitada.",
                "", "", "", "", "", "", 0
            ));
        }

        var token = BuildToken(usuario);
        _logger.LogInformation("Usuario {Email} autenticado como {Rol}", usuario.Email, usuario.Rol);

        return Ok(new AuthResponse(
            true,
            "Inicio de sesión exitoso.",
            token,
            usuario.Rol,
            usuario.Nombre,
            usuario.Email,
            usuario.Telefono,
            usuario.Direccion,
            usuario.Id
        ));
    }

    /// <summary>
    /// Registro de nuevo cliente en Brostería La Granja
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { mensaje = "Nombre, correo y contraseña son obligatorios." });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { mensaje = "La contraseña debe tener mínimo 6 caracteres." });
        }

        var emailNorm = request.Email.Trim().ToLower();
        var existe = await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNorm);
        if (existe)
        {
            return Conflict(new { mensaje = "Este correo ya está registrado." });
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Email = emailNorm,
            PasswordHash = LaGranjaDbContext.HashPassword(request.Password),
            Telefono = request.Telefono?.Trim() ?? "",
            Direccion = request.Direccion?.Trim() ?? "",
            Rol = "cliente",
            Activo = true,
            CreadoEn = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var token = BuildToken(usuario);
        _logger.LogInformation("Nuevo cliente registrado: {Email}", usuario.Email);

        return Created($"/api/auth/usuarios", new AuthResponse(
            true,
            "¡Cuenta creada exitosamente!",
            token,
            usuario.Rol,
            usuario.Nombre,
            usuario.Email,
            usuario.Telefono,
            usuario.Direccion,
            usuario.Id
        ));
    }

    /// <summary>
    /// Listado de usuarios registrados (uso administrativo)
    /// </summary>
    [HttpGet("usuarios")]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _db.Usuarios
            .OrderBy(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Email,
                u.Telefono,
                u.Direccion,
                u.Rol,
                u.Activo,
                u.CreadoEn
            })
            .ToListAsync();

        return Ok(usuarios);
    }
}
