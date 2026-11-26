namespace LaGranjaApi.Models;

/// <summary>
/// Pedido y boleta electrónica emitida por Brostería La Granja.
/// </summary>
public class Pedido
{
    public int Id { get; set; }
    public string NroBoleta { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string TipoEntrega { get; set; } = "Delivery";
    public string Direccion { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = "Yape / Plin";
    public string CuponAplicado { get; set; } = string.Empty;
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string ItemsJson { get; set; } = "[]";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

public record PedidoItemDto(
    int? ProductoId,
    string Nombre,
    decimal Precio,
    int Cantidad
);

public record PedidoCreateDto(
    string? NroBoleta,
    string? UserId,
    string ClienteNombre,
    string? ClienteEmail,
    string? Dni,
    string? Telefono,
    string? TipoEntrega,
    string? Direccion,
    string? MetodoPago,
    string? CuponAplicado,
    decimal Descuento,
    decimal Subtotal,
    decimal Igv,
    decimal Total,
    string? Estado,
    string? ItemsJson,
    List<PedidoItemDto>? Items = null
);

public record PedidoEstadoDto(string NuevoEstado);
