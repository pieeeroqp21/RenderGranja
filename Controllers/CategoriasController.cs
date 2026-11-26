using LaGranjaApi.Data;
using LaGranjaApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LaGranjaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Categoria>> GetAll() => Ok(LaGranjaData.Categorias);

    [HttpGet("{id:int}")]
    public ActionResult<Categoria> GetById(int id)
    {
        var item = LaGranjaData.Categorias.FirstOrDefault(x => x.Id == id);
        return item == null
            ? NotFound(new { mensaje = $"No existe categoría con ID {id}" })
            : Ok(item);
    }
}
