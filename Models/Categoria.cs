namespace LaGranjaApi.Models;

/// <summary>
/// Categoría gastronómica de la carta de Brostería La Granja.
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public int Orden { get; set; }
}
