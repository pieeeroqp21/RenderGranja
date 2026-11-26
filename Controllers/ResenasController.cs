using LaGranjaApi.Data;
using LaGranjaApi.Models;
using LaGranjaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResenasController : ControllerBase
{
    private readonly LaGranjaDbContext _db;
    private readonly FirebaseService _firebaseService;

    public ResenasController(LaGranjaDbContext db, FirebaseService firebaseService)
    {
        _db = db;
        _firebaseService = firebaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Resena>>> GetAll()
    {
        var resenas = await _db.Resenas.OrderByDescending(r => r.Fecha).ToListAsync();
        return Ok(resenas);
    }

    [HttpPost]
    public async Task<ActionResult<Resena>> Create([FromBody] Resena nuevaResena)
    {
        if (string.IsNullOrWhiteSpace(nuevaResena.Nombre) || string.IsNullOrWhiteSpace(nuevaResena.Comentario))
        {
            return BadRequest(new { mensaje = "El nombre y comentario son obligatorios." });
        }

        nuevaResena.Nombre = nuevaResena.Nombre.Trim();
        nuevaResena.Comentario = nuevaResena.Comentario.Trim();
        nuevaResena.Estrellas = Math.Clamp(nuevaResena.Estrellas, 1, 5);
        nuevaResena.Fecha = DateTime.UtcNow;

        _db.Resenas.Add(nuevaResena);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarResenaEnNubeAsync(nuevaResena);
        }

        return Created($"/api/resenas/{nuevaResena.Id}", nuevaResena);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var resena = await _db.Resenas.FirstOrDefaultAsync(r => r.Id == id);
        if (resena == null) return NotFound(new { mensaje = $"No existe reseña con ID {id}" });

        _db.Resenas.Remove(resena);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.EliminarResenaEnNubeAsync(id);
        }

        return Ok(new { mensaje = "Reseña eliminada correctamente.", idEliminado = id });
    }
}
