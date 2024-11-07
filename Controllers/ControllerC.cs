using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerC : Controller
    {
        private readonly MedifinderContext _baseDatos;

        public ControllerC(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        [HttpGet]
        [Route("sumarNumeros/{numero}")]
        public IActionResult SumarNumeros(int numero)
        {
            int resultado = numero + numero;
            return Ok(new { resultado });
        }
    }
}
