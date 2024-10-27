using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarTiposSuscripciones;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiposSuscripcionesController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public TiposSuscripcionesController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Registrar tipo suscripcion ----------------------------------------
        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarTipoSuscripcion([FromBody] TipoSuscripcionDTO tipoSuscripcionDTO)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tipoSuscripcionNuevo = new TipoSuscripcion
                {
                    Nombre = tipoSuscripcionDTO.Nombre,
                    Descripcion = tipoSuscripcionDTO.Descripcion,
                    Precio = tipoSuscripcionDTO.Precio,
                    Duracion = tipoSuscripcionDTO.Duracion,
                    Estatus = "1"
                };

                _baseDatos.TipoSuscripcion.Add(tipoSuscripcionNuevo);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "El tipo de suscripcion ha sido registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Modificar tipo suscripcion ----------------------------------------
        [HttpPut]
        [Route("Modificar/{id}")]
        public async Task<IActionResult> ModificarTipoSuscripcion(int id, [FromBody] TipoSuscripcionDTO tipoSuscripcionDTO)
        {
            // Valida que el modelo recibido esté correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(new { mensaje = "Datos no válidos.", estatus = "error", data = ModelState });
            }

            var tipoSuscripcionExistente = await _baseDatos.TipoSuscripcion.FindAsync(id);
            if (tipoSuscripcionExistente == null)
            {
                return NotFound(new { mensaje = $"Tipo de suscripción con id {id} no encontrado.", estatus = "error", data = new { } });
            }

            try
            {
                tipoSuscripcionExistente.Nombre = tipoSuscripcionDTO.Nombre;
                tipoSuscripcionExistente.Descripcion = tipoSuscripcionDTO.Descripcion;
                tipoSuscripcionExistente.Precio = tipoSuscripcionDTO.Precio;
                tipoSuscripcionExistente.Duracion = tipoSuscripcionDTO.Duracion;

                _baseDatos.TipoSuscripcion.Update(tipoSuscripcionExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { mensaje = "Registro actualizado exitosamente.", estatus = "success", data = tipoSuscripcionExistente });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno del servidor: {ex.Message}", estatus = "error", data = new { } });
            }
        }


        //Detalles de suscripcion -------------------------------------------
        [HttpGet]
        [Route("Detalles/{id}")]
        public async Task<IActionResult> DetallesTipoSuscripcion(int id)
        {
            try
            {
                //Consulta para sacar todos los medicos permitidos
                var tipoSuscripcion = await _baseDatos.TipoSuscripcion
                    .Where(ts => ts.Id == id)
                    .Select(ts => new
                    {
                        ts.Id,
                        ts.Nombre,
                        ts.Descripcion,
                        ts.Precio,
                        ts.Duracion,
                        ts.Estatus
                    })
                    .FirstOrDefaultAsync();

                if (tipoSuscripcion == null)
                {
                    return NotFound($"Tipo de suscripción con id {id} no encontrado.");
                }

                return Ok(tipoSuscripcion);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Eliminar tipo de suscripcion
        [HttpPut]
        [Route("Eliminar/{id}")]
        public async Task<IActionResult> EliminarTipoSuscripcion(int id)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipoSuscripcionExistente = await _baseDatos.TipoSuscripcion.FindAsync(id);
            if (tipoSuscripcionExistente == null)
            {
                return NotFound($"Tipo de suscripción con id {id} no encontrado.");
            }

            try
            {
                tipoSuscripcionExistente.Estatus = "0";

                _baseDatos.TipoSuscripcion.Update(tipoSuscripcionExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Registro actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Listado de tipos de suscripcion ------------------------------------
        [HttpGet]
        [Route("ObtenerTiposSuscripcionActivos")]
        public async Task<IActionResult> ObtenerTiposSuscripcionActivos()
        {
            var tiposSuscripcionActivos = await _baseDatos.TipoSuscripcion
                .Select(ts => new
                {
                    ts.Id,
                    ts.Nombre,
                    ts.Descripcion,
                    ts.Precio,
                    ts.Duracion,
                    Estatus = ts.Estatus == "1" ? "Activo" : "Inactivo"
                })
                .ToListAsync();

            // Filtrar solo los activos
            var tiposActivos = tiposSuscripcionActivos
                .Where(ts => ts.Estatus == "Activo")
                .ToList();

            if (tiposActivos.Count == 0)
            {
                return NotFound(new { message = "No se encontraron registros.", estatus = "0", data = new List<object>() });
            }

            return Ok(new
            {
                message = "Consulta realizada con éxito.",
                estatus = "success",
                data = tiposActivos
            });
        }

        [HttpGet]
        [Route("ObtenerTiposSuscripcion")]
        public async Task<IActionResult> ObtenerTiposSuscripcion()
        {
            var tiposSuscripciones = await _baseDatos.TipoSuscripcion
                .Select(ts => new
                {
                    ts.Id,
                    ts.Nombre,
                    ts.Descripcion,
                    ts.Precio,
                    ts.Duracion,
                    Estatus = ts.Estatus == "1" ? "Activo" : "Inactivo"
                })
                .ToListAsync();


            if (tiposSuscripciones.Count == 0)
            {
                return NotFound(new { message = "No se encontraron registros.", estatus = "0", data = new List<object>() });
            }

            return Ok(new
            {
                message = "Consulta realizada con éxito.",
                estatus = "success",
                data = tiposSuscripciones
            });
        }


        [HttpPut]
        [Route("Desactivar/{id}")]
        public async Task<IActionResult> DesactivarTipoSuscripcion(int id)
        {
            var tipoSuscripcionExistente = await _baseDatos.TipoSuscripcion.FindAsync(id);
            if (tipoSuscripcionExistente == null)
            {
                return NotFound(new
                {
                    mensaje = $"Tipo de suscripción con id {id} no encontrado.",
                    estatus = "error",
                    data = new { }
                });
            }
            try
            {
                tipoSuscripcionExistente.Estatus = "0";

                _baseDatos.TipoSuscripcion.Update(tipoSuscripcionExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Tipo de suscripción desactivado exitosamente.",
                    estatus = "success",
                    data = tipoSuscripcionExistente
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    estatus = "error",
                    data = new { }
                });
            }
        }

        [HttpPut]
        [Route("Activar/{id}")]
        public async Task<IActionResult> ActivarTipoSuscripcion(int id)
        {
            var tipoSuscripcionExistente = await _baseDatos.TipoSuscripcion.FindAsync(id);
            if (tipoSuscripcionExistente == null)
            {
                return NotFound(new
                {
                    mensaje = $"Tipo de suscripción con id {id} no encontrado.",
                    estatus = "error",
                    data = new { }
                });
            }
            try
            {
                tipoSuscripcionExistente.Estatus = "1";

                _baseDatos.TipoSuscripcion.Update(tipoSuscripcionExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Tipo de suscripción desactivado exitosamente.",
                    estatus = "success",
                    data = tipoSuscripcionExistente
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    estatus = "error",
                    data = new { }
                });
            }
        }



    }
}
