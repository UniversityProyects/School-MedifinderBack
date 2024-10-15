using Microsoft.AspNetCore.Mvc;

namespace MediFinder_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CmrMobilController : ControllerBase
    {
        // Método GET para obtener un mensaje simple
        [HttpGet("hello")]
        public IActionResult GetHello()
        {
            return Ok("Hello from CmrMobilController!");
        }

        // Método POST que acepta datos y devuelve una respuesta
        [HttpPost("create")]
        public IActionResult CreateItem([FromBody] string item)
        {
            // Aquí puedes agregar la lógica para procesar el item recibido
            return Ok($"Item '{item}' creado con éxito.");
        }
    }
}
