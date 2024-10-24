using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarTiposSuscripciones;

namespace MediFinder_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CMRMovilController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        //Contructor del controlador
        public CMRMovilController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Método get para obtener el listado de medicos ordenados de menor a mayor
        [HttpGet]
        [Route("PromedioCalificacionMedico")]
        public async Task<IActionResult> ObtenerPromedioCalificacionMedico()
        {
            try
            {

                //Sacamos el promedio con base en una consulta
                var promedio = await (
                    from med in _baseDatos.Medicos
                    join c in _baseDatos.Cita on med.Id equals c.IdMedico into citas
                    from c in citas.DefaultIfEmpty()
                    join cm in _baseDatos.CalificacionMedicos on c.Id equals cm.IdCita into calificaciones
                    from cm in calificaciones.DefaultIfEmpty()
                    group new { cm, med } by new { med.Id, med.Nombre, med.Apellido } into grouped
                    select new
                    {
                        Id = grouped.Key.Id,
                        Nombre = grouped.Key.Nombre,
                        Apellido = grouped.Key.Apellido,
                        PromedioPuntuacion = Math.Round(
                            (float)grouped.Sum(g => g.cm != null ? g.cm.Puntuacion : 0) /
                            (grouped.Count(g => g.cm != null) > 0 ? grouped.Count(g => g.cm != null) : 1),
                            2
                        ),
                        Estatus = grouped.Max(g => g.med.Estatus),
                        CantidadComentarios = grouped.Count(x => x.cm.Comentarios != null)
                    }
                ).OrderBy(r => r.PromedioPuntuacion).ToListAsync();

                return Ok(promedio);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }


        [HttpGet]
        [Route("ListadoComentariosPorMedico/{idMedico}")]
        public async Task<IActionResult> ObtenerListadoComentariosMedico(int idMedico)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound(new { CodigoError = 404, mensaje = $"El médico ingresado no exite" });
                }

                //Sacamos el promedio con base en una consulta
                var resultado = await (
                    from c in _baseDatos.Cita
                    join cm in _baseDatos.CalificacionMedicos on c.Id equals cm.IdCita // INNER JOIN
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id // INNER JOIN
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id // INNER JOIN

                    where m.Id == idMedico

                    select new
                    {
                        Id_Cita = c.Id,
                        IdMedico = c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        FechaInicio = c.FechaInicio,  // Contiene fecha y hora
                        FechaFin = c.FechaFin,        // Contiene fecha y hora
                        Puntuacion = cm.Puntuacion,    // Ahora obligatorio, ya no puede ser null
                        FechaPuntuacion = (DateOnly?)cm.Fecha,  // Solo fecha
                        Comentarios = cm.Comentarios
                    }
                ).ToListAsync();


                return Ok(resultado);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }


        [Route("DetallesComentariosPorCita/{idCita}")]
        [HttpGet]
        public async Task<ActionResult<int>> ObtenerComentarioPorCita(int idCita)
        {
            try
            {
                // Verificar si la cita existe en la BD
                var existeCita = await _baseDatos.Cita.FirstOrDefaultAsync(e => e.Id == idCita);
                if (existeCita == null)
                {
                    return NotFound(new { CodigoError = 404, mensaje = $"La cita solicitada no existe" });
                }

                // Verificar si ya existe una calificación para la cita
                var calificacionExistente = await (
                    from c in _baseDatos.Cita
                    join cm in _baseDatos.CalificacionMedicos on c.Id equals cm.IdCita // INNER JOIN
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id // INNER JOIN
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id // INNER JOIN

                    where c.Id == idCita

                    select new
                    {
                        Id_Cita = c.Id,
                        IdMedico = c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        FechaInicio = c.FechaInicio,  // Contiene fecha y hora
                        FechaFin = c.FechaFin,        // Contiene fecha y hora
                        Estatus_Cita = c.Estatus,
                        Puntuacion = cm.Puntuacion,    // Ahora obligatorio, ya no puede ser null
                        FechaPuntuacion = (DateOnly?)cm.Fecha,   // Solo fecha
                        Comentarios = cm.Comentarios
                    }
                ).FirstOrDefaultAsync();

                if (calificacionExistente != null)
                {
                    return Ok(calificacionExistente);
                }

                // Retornar 0 si no existe calificación
                return NotFound(new { CodigoHttp = 404, mensaje = $"No existe calificación para la cita solicitada." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Método get para obtener el listado de tipos de suscripciones
        [HttpGet]
        [Route("ObtenerTiposSuscripcionActivos")]
        public async Task<IActionResult> ObtenerListadoTiposSuscripciones()
        {
            try
            {
                var resultados = from ts in _baseDatos.TipoSuscripcion
                                 where ts.Estatus == "1"
                                 select new
                                 {
                                     Id = ts.Id,
                                     Nombre = ts.Nombre,
                                     Descripcion = ts.Descripcion,
                                     Precio = ts.Precio,
                                     Duracion = ts.Duracion
                                 };


                return Ok(resultados);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Registrar tipo suscripcion ----------------------------------------
        [HttpPost]
        [Route("RegistrarTipoSuscripcion")]
        public async Task<IActionResult> RegistrarTipoSuscripcion([FromBody] TipoSuscripcionDTO tipoSuscripcionDTO)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(new { CodigoHttp = 400, mensaje = $"La petición no contiene la estructura requerida." });
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

                return Ok(new { CodigoHttp = 200, mensaje = "El tipo de suscripcion ha sido registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Modificar tipo suscripcion ----------------------------------------
        [HttpPut]
        [Route("ModificarTipoSuscripcion/{id}")]
        public async Task<IActionResult> ModificarTipoSuscripcion(int id, [FromBody] TipoSuscripcionDTO tipoSuscripcionDTO)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(new { CodigoHttp = 400, mensaje = $"La petición no contiene la estructura requerida." });
            }

            var tipoSuscripcionExistente = await _baseDatos.TipoSuscripcion.FindAsync(id);
            if (tipoSuscripcionExistente == null)
            {
                return NotFound(new { CodigoHttp = 404, mensaje = $"Tipo de suscripción con id {id} no encontrado." });
            }

            try
            {
                tipoSuscripcionExistente.Nombre = tipoSuscripcionDTO.Nombre;
                tipoSuscripcionExistente.Descripcion = tipoSuscripcionDTO.Descripcion;
                tipoSuscripcionExistente.Precio = tipoSuscripcionDTO.Precio;
                tipoSuscripcionExistente.Duracion = tipoSuscripcionDTO.Duracion;

                _baseDatos.TipoSuscripcion.Update(tipoSuscripcionExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { CodigoHttp = 200, message = "Registro actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Detalles de suscripcion -------------------------------------------
        [HttpGet]
        [Route("DetallesTipoSuscripcion/{id}")]
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
                    return NotFound(new { CodigoHttp = 404, mensaje = $"Tipo de suscripción con id {id} no encontrado." });
                }

                return Ok(tipoSuscripcion);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Eliminar tipo de suscripcion
        [HttpPut]
        [Route("EliminarTipoSuscripcion/{id}")]
        public async Task<IActionResult> EliminarTipoSuscripcion(int id)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(new { CodigoHttp = 400, mensaje = $"La petición no contiene la estructura requerida." });
            }

            var tipoSuscripcionExistente = await _baseDatos.TipoSuscripcion.FindAsync(id);
            if (tipoSuscripcionExistente == null)
            {
                return NotFound(new { CodigoHttp = 404, mensaje = $"Tipo de suscripción con id {id} no encontrado." });
            }

            try
            {
                tipoSuscripcionExistente.Estatus = "0";

                _baseDatos.TipoSuscripcion.Update(tipoSuscripcionExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { CodigoHttp = 200, message = "Registro actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }
    }
}
