using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TiendaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelaAPIController : ControllerBase
    {
        private readonly BdtiendatelasContext ctx;

        public TelaAPIController(BdtiendatelasContext _ctx)
        {
            this.ctx = _ctx;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tela>>> GetTelas()
        {
            try
            {
                var telas = await ctx.ListarTelasAsync();
                return Ok(telas); // Devuelve las telas como un JSON con código HTTP 200
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //get por id
        [HttpGet("{id}")]
        public async Task<ActionResult<Tela>> GetTela(string id)
        {
            try
            {
                var tela = await ctx.BuscarTelaPorIDAsync(id);

                if (tela == null)
                {
                    return NotFound($"No se encontró ninguna tela con el código: {id}");
                }

                return Ok(tela);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET api/TelaAPI/iniciales/{iniciales}
        [HttpGet("iniciales/{iniciales}")]
        public async Task<ActionResult<IEnumerable<Tela>>> BuscarPorIniciales(string iniciales)
        {
            try
            {
                var telas = await ctx.BuscarTelaPorInicialesAsync(iniciales);

                if (telas == null || telas.Count == 0)
                {
                    return NotFound($"No se encontraron telas cuyo tipo comience con: {iniciales}");
                }

                return Ok(telas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // METODOS DE OPERACIONES-------------------------------
        [HttpPost("agregar-al-carrito")]
        public async Task<ActionResult<string>> AgregarProductoAlCarrito(int idUsuario, string codTel, int cantidad)
        {
            try
            {
                // Llamar al método del contexto para ejecutar el procedimiento almacenado
                var resultado = await ctx.AgregarProductoAlCarritoAsync(idUsuario, codTel, cantidad);
                return Ok(resultado); // Devuelve un mensaje de éxito
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //eliminar de la fila
        [HttpPost("eliminar-producto-carrito")]
        public async Task<IActionResult> EliminarProductoDelCarrito(int idUsuario, string codTel)
        {
            try
            {
                var resultado = await ctx.EliminarProductoDelCarritoAsync(idUsuario, codTel);
                return Ok(resultado); // Retorna un mensaje de éxito
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}"); // Maneja cualquier error
            }
        }

        //actualizar
        [HttpPost("actualizar-cantidad-producto")]
        public async Task<IActionResult> ActualizarCantidadProducto([FromBody] ActualizarCantidadProductoRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CodTel) || request.NuevaCantidad <= 0)
            {
                return BadRequest("Datos incorrectos.");
            }

            try
            {
                var resultado = await ctx.ActualizarCantidadProductoEnCarritoAsync(request.IdUsuario, request.CodTel, request.NuevaCantidad);
                return Ok(resultado); // Devuelve el mensaje de éxito
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la cantidad del producto: {ex.Message}");
            }
        }


        // detalle del carrito pendiente
        [HttpGet("detalle-carrito-pendiente/{idUsuario}")]
        public async Task<IActionResult> ObtenerDetalleCarritoPendiente(int idUsuario)
        {
            try
            {
                var detalles = await ctx.ListarDetalleCarritoPendienteAsync(idUsuario);
                if (detalles == null || detalles.Count == 0)
                {
                    return NotFound("No se encontraron productos en el carrito pendiente.");
                }

                return Ok(detalles); // Retorna la lista de detalles
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los detalles del carrito: {ex.Message}");
            }
        }





        // POST api/<TelaAPIController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<TelaAPIController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TelaAPIController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
