using LaGranjaApi.Data;
using LaGranjaApi.Models;
using LaGranjaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly LaGranjaDbContext _db;
    private readonly FirebaseService _firebaseService;

    public ReservasController(LaGranjaDbContext db, FirebaseService firebaseService)
    {
        _db = db;
        _firebaseService = firebaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reservas = await _db.ReservasMesas
            .OrderByDescending(r => r.CreadoEnUtc)
            .ToListAsync();
        return Ok(reservas);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReservaCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreCliente) || string.IsNullOrWhiteSpace(dto.Telefono) ||
            string.IsNullOrWhiteSpace(dto.Fecha) || string.IsNullOrWhiteSpace(dto.Hora))
        {
            return BadRequest(new { mensaje = "Nombre, teléfono, fecha y hora son obligatorios para reservar." });
        }

        var reserva = new ReservaMesa
        {
            Codigo = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100, 999)}",
            NombreCliente = dto.NombreCliente.Trim(),
            Telefono = dto.Telefono.Trim(),
            Email = dto.Email?.Trim().ToLower() ?? "",
            Fecha = dto.Fecha.Trim(),
            Hora = dto.Hora.Trim(),
            Personas = Math.Clamp(dto.Personas, 1, 30),
            Zona = string.IsNullOrWhiteSpace(dto.Zona) ? "Salón Principal" : dto.Zona.Trim(),
            Notas = dto.Notas?.Trim() ?? "",
            Estado = "Confirmada",
            CreadoEnUtc = DateTime.UtcNow
        };

        _db.ReservasMesas.Add(reserva);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarReservaEnNubeAsync(reserva);
        }

        return Created($"/api/reservas", new
        {
            mensaje = "¡Mesa reservada con éxito!",
            sincronizadaEnFirebase = _firebaseService.IsAvailable,
            reserva
        });
    }

    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> UpdateEstado(string codigo, [FromBody] ReservaEstadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NuevoEstado))
            return BadRequest(new { mensaje = "El nuevo estado es obligatorio." });

        var reserva = await _db.ReservasMesas.FirstOrDefaultAsync(r => r.Codigo.ToUpper() == codigo.Trim().ToUpper());
        if (reserva == null) return NotFound(new { mensaje = "Reserva no encontrada." });

        reserva.Estado = dto.NuevoEstado.Trim();
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarReservaEnNubeAsync(reserva);
        }

        return Ok(new { mensaje = "Estado de reserva actualizado.", reserva });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        var reserva = await _db.ReservasMesas.FirstOrDefaultAsync(r => r.Codigo.ToUpper() == codigo.Trim().ToUpper());
        if (reserva == null) return NotFound(new { mensaje = "Reserva no encontrada." });

        _db.ReservasMesas.Remove(reserva);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Reserva eliminada.", codigo });
    }
}
