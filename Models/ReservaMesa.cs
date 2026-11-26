namespace LaGranjaApi.Models;

/// <summary>
/// Reserva de mesa en el local de Brostería La Granja.
/// </summary>
public class ReservaMesa
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;
    public int Personas { get; set; } = 2;
    public string Zona { get; set; } = "Salón Principal";
    public string Notas { get; set; } = string.Empty;
    public string Estado { get; set; } = "Confirmada"; // Confirmada | Atendida | Cancelada
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
}

public record ReservaCreateDto(
    string NombreCliente,
    string Telefono,
    string? Email,
    string Fecha,
    string Hora,
    int Personas,
    string? Zona,
    string? Notas
);

public record ReservaEstadoDto(string NuevoEstado);
