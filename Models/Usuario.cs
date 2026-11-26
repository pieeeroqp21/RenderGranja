namespace LaGranjaApi.Models;

/// <summary>
/// Usuario registrado en el sistema (administrador o cliente).
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Rol { get; set; } = "cliente"; // "admin" | "cliente"
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Nombre,
    string Email,
    string Password,
    string? Telefono = null,
    string? Direccion = null
);

public record AuthResponse(
    bool Exito,
    string Mensaje,
    string Token,
    string Rol,
    string Nombre,
    string Email,
    string Telefono,
    string Direccion,
    int Id
);
