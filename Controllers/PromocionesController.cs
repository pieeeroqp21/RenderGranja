using LaGranjaApi.Data;
using LaGranjaApi.Models;
using LaGranjaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromocionesController : ControllerBase
{
    private readonly LaGranjaDbContext _db;
    private readonly FirebaseService _firebaseService;

    public PromocionesController(LaGranjaDbContext db, FirebaseService firebaseService)
    {
        _db = db;
        _firebaseService = firebaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Promocion>>> GetAll([FromQuery] bool soloActivas = false)
    {
        var query = _db.Promociones.AsQueryable();
        if (soloActivas) query = query.Where(p => p.Activa);
        return Ok(await query.OrderByDescending(p => p.Id).ToListAsync());
    }

    [HttpGet("validar/{codigo}")]
    public async Task<IActionResult> ValidarCupon(string codigo)
    {
        var norm = codigo.Trim().ToUpper();
        var promo = await _db.Promociones.FirstOrDefaultAsync(p => p.Activa && p.CodigoCupon.ToUpper() == norm);
        if (promo == null)
        {
            return NotFound(new { valida = false, mensaje = $"El cupón '{norm}' no es válido o ya expiró." });
        }

        return Ok(new
        {
            valida = true,
            mensaje = $"Cupón {promo.CodigoCupon} aplicado ({promo.PorcentajeDescuento}% de descuento).",
            promocion = promo
        });
    }

    [HttpPost]
    public async Task<ActionResult<Promocion>> Create([FromBody] Promocion nuevaPromo)
    {
        if (string.IsNullOrWhiteSpace(nuevaPromo.Titulo) || string.IsNullOrWhiteSpace(nuevaPromo.Detalle))
        {
            return BadRequest(new { mensaje = "El título y detalle de la promoción son obligatorios." });
        }

        if (nuevaPromo.PorcentajeDescuento < 0 || nuevaPromo.PorcentajeDescuento > 90)
        {
            return BadRequest(new { mensaje = "El porcentaje de descuento debe estar entre 0 y 90%." });
        }

        nuevaPromo.Titulo = nuevaPromo.Titulo.Trim();
        nuevaPromo.Detalle = nuevaPromo.Detalle.Trim();
        nuevaPromo.CodigoCupon = (nuevaPromo.CodigoCupon ?? "").Trim().ToUpper();
        nuevaPromo.Vigencia = string.IsNullOrWhiteSpace(nuevaPromo.Vigencia) ? "Promoción vigente" : nuevaPromo.Vigencia.Trim();
        nuevaPromo.Fecha = DateTime.UtcNow;

        _db.Promociones.Add(nuevaPromo);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarPromocionEnNubeAsync(nuevaPromo);
        }

        return Created($"/api/promociones/{nuevaPromo.Id}", nuevaPromo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Promocion promoActualizada)
    {
        var promo = await _db.Promociones.FirstOrDefaultAsync(p => p.Id == id);
        if (promo == null) return NotFound(new { mensaje = $"No existe promoción con ID {id}" });

        if (!string.IsNullOrWhiteSpace(promoActualizada.Titulo))
            promo.Titulo = promoActualizada.Titulo.Trim();
        if (!string.IsNullOrWhiteSpace(promoActualizada.Detalle))
            promo.Detalle = promoActualizada.Detalle.Trim();
        promo.CodigoCupon = (promoActualizada.CodigoCupon ?? promo.CodigoCupon).Trim().ToUpper();
        promo.PorcentajeDescuento = promoActualizada.PorcentajeDescuento;
        promo.Vigencia = string.IsNullOrWhiteSpace(promoActualizada.Vigencia) ? promo.Vigencia : promoActualizada.Vigencia.Trim();
        promo.Activa = promoActualizada.Activa;

        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarPromocionEnNubeAsync(promo);
        }

        return Ok(promo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var promo = await _db.Promociones.FirstOrDefaultAsync(p => p.Id == id);
        if (promo == null) return NotFound(new { mensaje = $"No existe promoción con ID {id}" });

        _db.Promociones.Remove(promo);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.EliminarPromocionEnNubeAsync(id);
        }

        return Ok(new { mensaje = "Promoción eliminada correctamente", idEliminado = id });
    }
}
