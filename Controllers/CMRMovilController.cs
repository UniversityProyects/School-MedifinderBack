using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                return StatusCode(500, new { CodigoError = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
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
                    return NotFound(new { CodigoError = 404, mensaje = $"El médico ingresado no exite" } );
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
                return StatusCode(500, new { CodigoError = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
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
                return NotFound(new { CodigoError = 404, mensaje = $"No existe calificación para la cita solicitada." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoError = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

    }
}
