using MediFinder_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;


namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : Controller
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public EspecialidadesController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        // GET: api/Especialidades/ConsultarEspecialidades
        [HttpGet]
        [Route("ConsultarEspecialidades")]
        public async Task<IActionResult> ConsultarEspecialidades()
        {
            try
            {
                var especialidades = await _baseDatos.Especialidad.ToListAsync();
                return Ok(especialidades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Especialidades/InsertarEspecialidad
        [HttpPost]
        [Route("InsertarEspecialidad")]
        public async Task<IActionResult> InsertarEspecialidad([FromBody] string nombreEspecialidad)
        {
            if (string.IsNullOrEmpty(nombreEspecialidad))
            {
                return BadRequest("El nombre de la especialidad es requerido");
            }

            try
            {
                var especialidad = new Especialidad
                {
                    Nombre = nombreEspecialidad
                };

                _baseDatos.Especialidad.Add(especialidad);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Especialidad insertada correctamente", especialidad.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


    }
}
