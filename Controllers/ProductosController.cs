using LaGranjaApi.Data;
using LaGranjaApi.Models;
using LaGranjaApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly LaGranjaDbContext _db;
    private readonly FirebaseService _firebaseService;

    public ProductosController(LaGranjaDbContext db, FirebaseService firebaseService)
    {
        _db = db;
        _firebaseService = firebaseService;
    }

    /// <summary>
    /// Obtiene todos los platos de la carta con filtros opcionales por categoría o texto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll([FromQuery] string? categoria = null, [FromQuery] string? q = null)
    {
        var query = _db.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria) && !categoria.Equals("Todos", StringComparison.OrdinalIgnoreCase))
        {
            var catNorm = categoria.Trim().ToLower();
            query = query.Where(p => p.Categoria.ToLower() == catNorm);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(term) ||
                p.Descripcion.ToLower().Contains(term) ||
                p.Categoria.ToLower().Contains(term) ||
                p.Codigo.ToLower().Contains(term));
        }

        var productos = await query.OrderBy(p => p.Id).ToListAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene un plato por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"No existe un plato con ID {id}" });
        }
        return Ok(producto);
    }

    /// <summary>
    /// Registra un nuevo plato en la carta
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto nuevoProducto)
    {
        if (string.IsNullOrWhiteSpace(nuevoProducto.Codigo))
            return BadRequest(new { mensaje = "El código del plato es obligatorio" });

        if (string.IsNullOrWhiteSpace(nuevoProducto.Nombre))
            return BadRequest(new { mensaje = "El nombre del plato es obligatorio" });

        if (nuevoProducto.Precio < 0)
            return BadRequest(new { mensaje = "El precio no puede ser negativo" });

        if (nuevoProducto.Stock < 0)
            return BadRequest(new { mensaje = "El stock no puede ser negativo" });

        if (nuevoProducto.Descuento < 0 || nuevoProducto.Descuento > 90)
            return BadRequest(new { mensaje = "El descuento debe estar entre 0 y 90%" });

        var codigoNorm = nuevoProducto.Codigo.Trim().ToUpper();
        var existeCodigo = await _db.Productos.AnyAsync(x => x.Codigo.ToUpper() == codigoNorm);
        if (existeCodigo)
            return Conflict(new { mensaje = $"Ya existe un plato con el código '{codigoNorm}'" });

        nuevoProducto.Codigo = codigoNorm;
        nuevoProducto.Nombre = nuevoProducto.Nombre.Trim();
        nuevoProducto.Categoria = string.IsNullOrWhiteSpace(nuevoProducto.Categoria) ? "Broaster" : nuevoProducto.Categoria.Trim();
        nuevoProducto.CategoriaId = LaGranjaData.ResolveCategoriaId(nuevoProducto.Categoria);
        nuevoProducto.Porcion = nuevoProducto.Porcion?.Trim() ?? "";
        nuevoProducto.Imagen = nuevoProducto.Imagen?.Trim() ?? "";
        nuevoProducto.Descripcion = nuevoProducto.Descripcion?.Trim() ?? "";
        nuevoProducto.Fecha = DateTime.UtcNow;

        _db.Productos.Add(nuevoProducto);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarProductoEnNubeAsync(nuevoProducto);
        }

        return CreatedAtAction(nameof(GetById), new { id = nuevoProducto.Id }, nuevoProducto);
    }

    /// <summary>
    /// Actualiza un plato existente en la carta
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto productoActualizado)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"No existe un plato con ID {id}" });
        }

        if (string.IsNullOrWhiteSpace(productoActualizado.Codigo))
            return BadRequest(new { mensaje = "El código es obligatorio" });

        if (string.IsNullOrWhiteSpace(productoActualizado.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio" });

        if (productoActualizado.Precio < 0)
            return BadRequest(new { mensaje = "El precio no puede ser negativo" });

        if (productoActualizado.Stock < 0)
            return BadRequest(new { mensaje = "El stock no puede ser negativo" });

        if (productoActualizado.Descuento < 0 || productoActualizado.Descuento > 90)
            return BadRequest(new { mensaje = "El descuento debe estar entre 0 y 90%" });

        var codigoNorm = productoActualizado.Codigo.Trim().ToUpper();
        var codigoDuplicado = await _db.Productos.AnyAsync(x => x.Codigo.ToUpper() == codigoNorm && x.Id != id);
        if (codigoDuplicado)
            return Conflict(new { mensaje = $"El código '{codigoNorm}' ya pertenece a otro plato" });

        producto.Codigo = codigoNorm;
        producto.Nombre = productoActualizado.Nombre.Trim();
        producto.Categoria = string.IsNullOrWhiteSpace(productoActualizado.Categoria) ? "Broaster" : productoActualizado.Categoria.Trim();
        producto.CategoriaId = LaGranjaData.ResolveCategoriaId(producto.Categoria);
        producto.Porcion = productoActualizado.Porcion?.Trim() ?? "";
        producto.Precio = productoActualizado.Precio;
        producto.Descuento = productoActualizado.Descuento;
        producto.Stock = productoActualizado.Stock;
        producto.Imagen = productoActualizado.Imagen?.Trim() ?? "";
        producto.Descripcion = productoActualizado.Descripcion?.Trim() ?? "";
        producto.Disponible = productoActualizado.Disponible;

        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.GuardarProductoEnNubeAsync(producto);
        }

        return Ok(producto);
    }

    /// <summary>
    /// Elimina un plato de la carta
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(x => x.Id == id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"No existe un plato con ID {id}" });
        }

        var codigoEliminado = producto.Codigo;
        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();

        if (_firebaseService.IsAvailable)
        {
            await _firebaseService.EliminarProductoEnNubeAsync(codigoEliminado);
        }

        return Ok(new { mensaje = "Plato eliminado correctamente", idEliminado = id });
    }
}
