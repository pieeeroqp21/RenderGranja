using System.Text.Json;
using LaGranjaApi.Data;
using LaGranjaApi.Models;
using LaGranjaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly LaGranjaDbContext _db;
    private readonly FirebaseService _firebaseService;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(LaGranjaDbContext db, FirebaseService firebaseService, ILogger<PedidosController> logger)
    {
        _db = db;
        _firebaseService = firebaseService;
        _logger = logger;
    }

    /// <summary>
    /// Listado general de pedidos y boletas (filtrable por correo o userId)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? email = null, [FromQuery] string? userId = null)
    {
        var query = _db.Pedidos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailNorm = email.Trim().ToLower();
            query = query.Where(p => p.ClienteEmail.ToLower() == emailNorm);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var uid = userId.Trim();
            query = query.Where(p => p.UserId == uid);
        }

        var pedidos = await query
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

        return Ok(pedidos);
    }

    /// <summary>
    /// Consulta de pedido por número de boleta
    /// </summary>
    [HttpGet("{nroBoleta}")]
    public async Task<IActionResult> GetByBoleta(string nroBoleta)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.NroBoleta.ToUpper() == nroBoleta.Trim().ToUpper());
        if (pedido == null) return NotFound(new { mensaje = "Boleta no encontrada." });
        return Ok(pedido);
    }

    /// <summary>
    /// Registro de nuevo pedido y emisión de boleta electrónica con descuento de stock
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PedidoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClienteNombre))
        {
            return BadRequest(new { mensaje = "El nombre del cliente es obligatorio." });
        }

        string itemsJsonFinal = dto.ItemsJson ?? "[]";
        if (dto.Items != null && dto.Items.Count > 0)
        {
            itemsJsonFinal = JsonSerializer.Serialize(dto.Items, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        var nroBoleta = string.IsNullOrWhiteSpace(dto.NroBoleta)
            ? $"B001-{Random.Shared.Next(100000, 999999)}"
            : dto.NroBoleta.Trim().ToUpper();

        // Evitar colisión de boleta
        if (await _db.Pedidos.AnyAsync(p => p.NroBoleta == nroBoleta))
        {
            nroBoleta = $"B001-{Random.Shared.Next(100000, 999999)}";
        }

        var totalFinal = dto.Total;
        var subtotalFinal = dto.Subtotal > 0 ? dto.Subtotal : Math.Round(totalFinal / 1.18m, 2);
        var igvFinal = dto.Igv > 0 ? dto.Igv : Math.Round(totalFinal - subtotalFinal, 2);

        var pedido = new Pedido
        {
            NroBoleta = nroBoleta,
            UserId = dto.UserId?.Trim() ?? "",
            ClienteNombre = dto.ClienteNombre.Trim(),
            ClienteEmail = dto.ClienteEmail?.Trim().ToLower() ?? "",
            Dni = dto.Dni?.Trim() ?? "",
            Telefono = dto.Telefono?.Trim() ?? "",
            TipoEntrega = string.IsNullOrWhiteSpace(dto.TipoEntrega) ? "Delivery" : dto.TipoEntrega.Trim(),
            Direccion = dto.Direccion?.Trim() ?? "Jr. Real 742, Huancayo",
            MetodoPago = string.IsNullOrWhiteSpace(dto.MetodoPago) ? "Yape / Plin" : dto.MetodoPago.Trim(),
            CuponAplicado = dto.CuponAplicado?.Trim().ToUpper() ?? "",
            Descuento = dto.Descuento,
            Subtotal = subtotalFinal,
            Igv = igvFinal,
            Total = totalFinal,
            Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado.Trim(),
            ItemsJson = itemsJsonFinal,
            Fecha = DateTime.UtcNow
        };

        _db.Pedidos.Add(pedido);

        // Descontar stock de platos registrados en la carta
        var productosModificados = new List<Producto>();
        try
        {
            using var doc = JsonDocument.Parse(pedido.ItemsJson);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    int qty = el.TryGetProperty("cantidad", out var qtyProp) && qtyProp.TryGetInt32(out int q) && q > 0 ? q : 1;
                    Producto? prod = null;

                    if (el.TryGetProperty("productoId", out var idProp) && idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out int prodId))
                    {
                        prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == prodId);
                    }
                    else if (el.TryGetProperty("nombre", out var nomProp))
                    {
                        var nom = nomProp.GetString()?.Trim().ToLower();
                        if (!string.IsNullOrEmpty(nom))
                        {
                            prod = await _db.Productos.FirstOrDefaultAsync(p => p.Nombre.ToLower() == nom);
                        }
                    }

                    if (prod != null && prod.Stock > 0)
                    {
                        prod.Stock = Math.Max(0, prod.Stock - qty);
                        if (prod.Stock == 0) prod.Disponible = false;
                        productosModificados.Add(prod);
                    }
                }
            }
        }
        catch (JsonException) { }

        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarPedidoEnNubeAsync(pedido);
            foreach (var prod in productosModificados)
            {
                await _firebaseService.GuardarProductoEnNubeAsync(prod);
            }
        }

        _logger.LogInformation("Boleta {Boleta} emitida por S/ {Total}", pedido.NroBoleta, pedido.Total);

        return CreatedAtAction(nameof(GetByBoleta), new { nroBoleta = pedido.NroBoleta }, pedido);
    }

    /// <summary>
    /// Actualiza el estado de preparación o entrega del pedido
    /// </summary>
    [HttpPut("{nroBoleta}/estado")]
    public async Task<IActionResult> UpdateEstado(string nroBoleta, [FromBody] PedidoEstadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NuevoEstado))
            return BadRequest(new { mensaje = "El nuevo estado es obligatorio." });

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.NroBoleta.ToUpper() == nroBoleta.Trim().ToUpper());
        if (pedido == null) return NotFound(new { mensaje = "Boleta no encontrada." });

        pedido.Estado = dto.NuevoEstado.Trim();
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarPedidoEnNubeAsync(pedido);
        }

        return Ok(new { mensaje = "Estado actualizado correctamente.", pedido });
    }

    [HttpDelete("{nroBoleta}")]
    public async Task<IActionResult> Delete(string nroBoleta)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.NroBoleta.ToUpper() == nroBoleta.Trim().ToUpper());
        if (pedido == null) return NotFound(new { mensaje = "Boleta no encontrada." });

        _db.Pedidos.Remove(pedido);
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Pedido eliminado.", nroBoleta });
    }
}
