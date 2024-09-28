using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using static MediFinder_Backend.ModelosEspeciales.RegistrarCalificacionMedicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalificacionMedicosController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        //Contructor del controlador
        public CalificacionMedicosController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        [Route("ExisteCalificacionParaCita/{idCita}")]
        [HttpGet]
        public async Task<ActionResult<int>> ExisteCalificacionParaCita(int idCita)
        {
            try
            {
                // Verificar si ya existe una calificación para la cita
                var calificacionExistente = await _baseDatos.CalificacionMedicos
                    .FirstOrDefaultAsync(c => c.IdCita == idCita);

                //Validar que el Id de la cita recibido si existe en la BD
                var existeCita = await _baseDatos.Cita.FirstOrDefaultAsync(e => e.Id == idCita);
                if (existeCita == null)
                {
                    return NotFound($"No existe la cita");
                }

                if (calificacionExistente != null)
                {
                    // Ya existe una calificación
                    return Ok(1);
                }

                // No existe calificación
                return Ok(0);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [Route("ObtenerCalificacionPorCita/{idCita}")]
        [HttpGet]
        public async Task<ActionResult<int>> ObtenerCalificacionPorCita(int idCita)
        {
            try
            {
                // Verificar si la cita existe en la BD
                var existeCita = await _baseDatos.Cita.FirstOrDefaultAsync(e => e.Id == idCita);
                if (existeCita == null)
                {
                    return NotFound("No existe la cita.");
                }

                // Verificar si ya existe una calificación para la cita
                var calificacionExistente = await _baseDatos.CalificacionMedicos
                    .Where(c => c.IdCita == idCita)
                    .Select(c => c.Puntuacion)
                    .FirstOrDefaultAsync();

                if (calificacionExistente != 0)
                {
                    return Ok(calificacionExistente);
                }

                // Retornar 0 si no existe calificación
                return NotFound($"No existe calificacion para la cita");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }





        //Registrar calificación del médico -----------------------------------------------------
        [HttpPost]
        [Route("RegistrarCalificacionMedico")]
        public async Task<ActionResult> RegistrarCalificacionMedico([FromBody] CalificacionMedicoDTO calificacionMedicoDTO)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Validar que el Id de la cita recibido si existe en la BD
                var existeCita = await _baseDatos.Cita.FirstOrDefaultAsync(e => e.Id == calificacionMedicoDTO.IdCita);
                if (existeCita == null)
                {
                    return NotFound($"No existe ningún registro de la cita recibida.");
                }

                if (calificacionMedicoDTO.Puntuacion < 0 || calificacionMedicoDTO.Puntuacion > 5)
                {
                    return BadRequest($"La puntación ingresada no es válida, debe de estar entre 1 y 5.");
                }

                //Formateamos el modelo de la calificacion
                var calificacionNueva = new CalificacionMedico
                {
                    IdCita = calificacionMedicoDTO.IdCita,
                    Puntuacion = calificacionMedicoDTO.Puntuacion,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Comentarios = calificacionMedicoDTO.Comentarios
                };

                // Guardar la calificacion en la base de datos
                _baseDatos.CalificacionMedicos.Add(calificacionNueva);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Calificación registrada con éxito", calificacionNueva.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Lista de calificacion del médico -----------------------------------------------------
        [HttpGet]
        [Route("ListadoCalificacionesMedico/{idMedico}")]
        public async Task<IActionResult> ObtenerListaCalificaciones(int idMedico)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound($"El médico ingresado no existe.");
                }

                //Ejecutamos la consulta
                var listaCalificacionesAgrupadas = await (
                    from cm in _baseDatos.CalificacionMedicos
                    join c in _baseDatos.Cita on cm.IdCita equals c.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id
                    where c.IdMedico == idMedico
                    group new { cm, c, p } by new
                    {
                        c.IdMedico,
                        m.Nombre,
                        m.Apellido
                    } into g
                    select new
                    {
                        IdMedico = g.Key.IdMedico,
                        NombreMedico = g.Key.Nombre,
                        ApellidoMedico = g.Key.Apellido,
                        Calificaciones = g.Select(x => new
                        {
                            x.cm.Id,
                            x.cm.IdCita,
                            x.cm.Puntuacion,
                            x.cm.Fecha,
                            IdPaciente = x.c.IdPaciente,
                            NombrePaciente = x.p.Nombre,
                            ApellidoPaciente = x.p.Apellido
                        }).ToList()
                    }).ToListAsync();

                if (listaCalificacionesAgrupadas.Count == 0)
                {
                    return NotFound(new { message = "No se encontraron calificaciones para el médico ingresado" });
                }

                // Retornamos la lista de resultados
                return Ok(listaCalificacionesAgrupadas);


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Detalle de calificacion del médico -----------------------------------------------------
        [HttpGet]
        [Route("DetallesCalificacionMedico/{id}")]
        public async Task<IActionResult> ObtenerDetallesCalificacionMedico(int id)
        {
            try
            {
                //Validar que el Id de la calificacion recibido si existe en la BD
                var existeCalificacion = await _baseDatos.CalificacionMedicos.FirstOrDefaultAsync(e => e.Id == id);
                if (existeCalificacion == null)
                {
                    return NotFound($"El la calificación solicitada no existe.");
                }

                //Ejecutamos la consulta
                var calificacionMedico = await (
                    from cm in _baseDatos.CalificacionMedicos
                    join c in _baseDatos.Cita on cm.IdCita equals c.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id
                    where cm.Id == id
                    select new
                    {
                        cm.Id,
                        cm.IdCita,
                        cm.Puntuacion,
                        cm.Fecha,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido
                    }
                ).ToListAsync();

                //retornamos el resultadof
                return Ok(calificacionMedico);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Promedio calificacion del médico -----------------------------------------------------
        [HttpGet]
        [Route("PromedioCalificacionMedico/{idMedico}")]
        public async Task<IActionResult> ObtenerPromedioCalificacionMedico(int idMedico)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound($"El médico ingresado no existe.");
                }

                //Sacamos el promedio con base en una consulta
                var promedio = await (
                    from cm in _baseDatos.CalificacionMedicos
                    join c in _baseDatos.Cita on cm.IdCita equals c.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    where c.IdMedico == idMedico
                    group new { cm, c, m } by new { c.IdMedico, m.Nombre, m.Apellido } into grouped
                    select new
                    {
                        IdMedico = grouped.Key.IdMedico,
                        Nombre = grouped.Key.Nombre,
                        Apellido = grouped.Key.Apellido,
                        PromedioPuntuacion = Math.Round((float)grouped.Sum(g => g.cm.Puntuacion) / grouped.Count(), 2)
                    }).ToListAsync();

                

                return Ok(promedio);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
