namespace LaGranjaApi.Models;

/// <summary>
/// Plato o producto disponible en la carta de Brostería La Granja.
/// </summary>
public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty; // Broaster | Alitas | Salchipapas | Combos | Bebidas
    public string Porcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Descuento { get; set; }
    public int Stock { get; set; } = 25;
    public string Imagen { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Disponible { get; set; } = true;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
