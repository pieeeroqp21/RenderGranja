namespace LaGranjaApi.Models;

/// <summary>
/// Reseña u opinión publicada por un cliente de la brostería.
/// </summary>
public class Resena
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Estrellas { get; set; } = 5;
    public string Comentario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
