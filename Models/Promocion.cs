namespace LaGranjaApi.Models;

/// <summary>
/// Promoción vigente y cupón de descuento aplicable en el carrito.
/// </summary>
public class Promocion
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public string CodigoCupon { get; set; } = string.Empty;
    public int PorcentajeDescuento { get; set; }
    public string Vigencia { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
