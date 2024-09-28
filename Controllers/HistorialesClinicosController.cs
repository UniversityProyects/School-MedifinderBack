using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarCita;
using static MediFinder_Backend.ModelosEspeciales.RegistrarHistorialClinico;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialesClinicosController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        //Contructor del controlador
        public HistorialesClinicosController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Registrar historial clinico del pacientes -----------------------------------------------------
        [HttpPost]
        [Route("RegistrarHistorialClinico")]
        public async Task<ActionResult> RegistrarHistorialClinicoPaciente([FromBody] HistorialClinicoDTO historialClinicoDTO)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Validar que el Id de la cita recibido si existe en la BD
                var existeCita = await _baseDatos.Cita.FirstOrDefaultAsync(e => e.Id == historialClinicoDTO.IdCita);
                if (existeCita == null)
                {
                    return NotFound($"No existe ningún registro del historial clínico recibido.");
                }

                //Formateamos el modelo del historial clinico
                var historialNuevo = new HistorialClinico
                {
                    IdCita = historialClinicoDTO.IdCita,
                    Observaciones = historialClinicoDTO.Observaciones,
                    Diagnostico = historialClinicoDTO.Diagnostico,
                    Padecimientos = historialClinicoDTO.Padecimientos,
                    Intervenciones = historialClinicoDTO.Intervenciones,
                    Fecha = historialClinicoDTO.Fecha,
                    PesoPaciente = historialClinicoDTO.PesoPaciente,
                    TallaPaciente = historialClinicoDTO.TallaPaciente,
                    GlucosaPaciente = historialClinicoDTO.GlucosaPaciente,
                    OxigenacionPaciente = historialClinicoDTO.OxigenacionPaciente,
                    PresionPaciente = historialClinicoDTO.PresionPaciente,
                    TemperaturaCorporalPaciente = historialClinicoDTO.TemperaturaCorporalPaciente
                };

                // Guardar la calificacion en la base de datos
                _baseDatos.HistorialClinico.Add(historialNuevo);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Historial registrado con éxito", historialNuevo.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Modificar Historial --------------------------------------------------------
        [HttpPut]
        [Route("ModificarHistorialClinico/{id}")]
        public async Task<IActionResult> ModificarHistorial(int id, [FromBody] HistorialClinicoDTO historialClinicoDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }

            try
            {
                //Validar que el Id del historial recibido si existe en la BD
                var existeHistorial = await _baseDatos.HistorialClinico.FirstOrDefaultAsync(e => e.Id == id);
                if (existeHistorial == null)
                {
                    return NotFound($"No existe ningún registro del historial clínico recibido.");
                }

                //Validar que el Id de la cita recibido si existe en la BD
                var existeCita = await _baseDatos.Cita.FirstOrDefaultAsync(e => e.Id == historialClinicoDTO.IdCita);
                if (existeCita == null)
                {
                    return NotFound($"No existe ningún registro de la cita recibida.");
                }

                //Actualizar la informacion del historials
                existeHistorial.IdCita = historialClinicoDTO.IdCita;
                existeHistorial.Observaciones = historialClinicoDTO.Observaciones;
                existeHistorial.Diagnostico = historialClinicoDTO.Diagnostico;
                existeHistorial.Padecimientos = historialClinicoDTO.Padecimientos;
                existeHistorial.Intervenciones = historialClinicoDTO.Intervenciones;
                existeHistorial.Fecha = historialClinicoDTO.Fecha;
                existeHistorial.PesoPaciente = historialClinicoDTO.PesoPaciente;
                existeHistorial.TallaPaciente = historialClinicoDTO.TallaPaciente;
                existeHistorial.GlucosaPaciente = historialClinicoDTO.GlucosaPaciente;
                existeHistorial.OxigenacionPaciente = historialClinicoDTO.OxigenacionPaciente;
                existeHistorial.PresionPaciente = historialClinicoDTO.PresionPaciente;
                existeHistorial.TemperaturaCorporalPaciente = historialClinicoDTO.TemperaturaCorporalPaciente;

                //Guardamos la informacion nueva
                await _baseDatos.SaveChangesAsync();

                //retornamos mensaje de confirmacion
                return Ok(new { message = $"El historial clínico con el Id {existeCita.Id} ha sido modificado correctamente." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Obtener listado de historial clinico por paciente --------------------------------------------------------
        [HttpPost]
        [Route("ObtenerHistorialClinicoPorPaciente/{idPaciente}")]
        public async Task<IActionResult> ObtenerHistorialPaciente(int idPaciente, [FromBody] ChecarHistorialDTO checarHistorialDTO)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == checarHistorialDTO.IdMedico);
                if (existeMedico == null)
                {
                    return NotFound($"El médico ingresado no existe.");
                }

                //Verificar que el medico tenga asignado el paciente
                var medicoAsignado = await _baseDatos.PacientesAsignados
                    .FirstOrDefaultAsync(pa => pa.IdPaciente == idPaciente && pa.IdMedico == checarHistorialDTO.IdMedico && pa.Estatus != "0");

                if (medicoAsignado == null)
                {
                    return Unauthorized($"No tiene el permiso para visualizar el historial del paciente.");
                }

                //Validar que el Id del paciente recibido si existe en la BD
                var existePaciente = await _baseDatos.Paciente.FirstOrDefaultAsync(e => e.Id == idPaciente);
                if (existePaciente == null)
                {
                    return NotFound($"El paciente ingresado no existe.");
                }

                //Hacemos la consulta y formateamos la respuesta
                var listaAgrupada = await (
                    from hc in _baseDatos.HistorialClinico
                    join c in _baseDatos.Cita on hc.IdCita equals c.Id
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    where c.IdPaciente == idPaciente
                    select new
                    {
                        Id = hc.Id,
                        IdCita = hc.IdCita,
                        FechaInicioCita = c.FechaInicio,
                        FechaFinCita = c.FechaFin,
                        EstatusCita = c.Estatus,
                        Observaciones = hc.Observaciones,
                        Diagnostico = hc.Diagnostico,
                        Padecimientos = hc.Padecimientos,
                        Intervenciones = hc.Intervenciones,
                        Fecha = hc.Fecha,
                        PesoPaciente = hc.PesoPaciente,
                        TallaPaciente = hc.TallaPaciente,
                        GlucosaPaciente = hc.GlucosaPaciente,
                        OxigenacionPaciente = hc.OxigenacionPaciente,
                        PresionPaciente = hc.PresionPaciente,
                        TemperaturaCorporalPaciente = hc.TemperaturaCorporalPaciente,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        IdMedico = c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido
                    })
                    .GroupBy(r => new { r.IdPaciente, r.NombrePaciente, r.ApellidoPaciente })
                    .Select(g => new
                    {
                        g.Key.IdPaciente,
                        g.Key.NombrePaciente,
                        g.Key.ApellidoPaciente,
                        HistorialClinico = g.Select(hc => new
                        {
                            hc.Id,
                            hc.IdCita,
                            hc.FechaInicioCita,
                            hc.FechaFinCita,
                            hc.EstatusCita,
                            hc.Observaciones,
                            hc.Diagnostico,
                            hc.Padecimientos,
                            hc.Intervenciones,
                            hc.Fecha,
                            hc.PesoPaciente,
                            hc.TallaPaciente,
                            hc.GlucosaPaciente,
                            hc.OxigenacionPaciente,
                            hc.PresionPaciente,
                            hc.TemperaturaCorporalPaciente,
                            hc.IdMedico,
                            hc.NombreMedico,
                            hc.ApellidoMedico
                        }).ToList()
                    }).ToListAsync();

                //Validamos si tiene hostorial sino retornamos un mensaje
                if (listaAgrupada.Count == 0)
                {
                    return Ok(new { message = "El paciente no cuenta con historial clínico" });
                }

                return Ok(listaAgrupada);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("ObtenerHistorialPaciente/{idPaciente}")]
        public async Task<IActionResult> ObtenerHistorialPaciente(int idPaciente)
        {
            try
            {
                // Validar que el Id del paciente recibido existe en la BD
                var existePaciente = await _baseDatos.Paciente.FirstOrDefaultAsync(e => e.Id == idPaciente);
                if (existePaciente == null)
                {
                    return NotFound($"El paciente con Id {idPaciente} no existe.");
                }

                // Hacemos la consulta y formateamos la respuesta
                var listaAgrupada = await (
                    from hc in _baseDatos.HistorialClinico
                    join c in _baseDatos.Cita on hc.IdCita equals c.Id
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    where c.IdPaciente == idPaciente
                    select new
                    {
                        Id = hc.Id,
                        IdCita = hc.IdCita,
                        FechaInicioCita = c.FechaInicio,
                        FechaFinCita = c.FechaFin,
                        EstatusCita = c.Estatus,
                        Observaciones = hc.Observaciones,
                        Diagnostico = hc.Diagnostico,
                        Padecimientos = hc.Padecimientos,
                        Intervenciones = hc.Intervenciones,
                        Fecha = hc.Fecha,
                        PesoPaciente = hc.PesoPaciente,
                        TallaPaciente = hc.TallaPaciente,
                        GlucosaPaciente = hc.GlucosaPaciente,
                        OxigenacionPaciente = hc.OxigenacionPaciente,
                        PresionPaciente = hc.PresionPaciente,
                        TemperaturaCorporalPaciente = hc.TemperaturaCorporalPaciente,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        IdMedico = c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido
                    })
                    .GroupBy(r => new { r.IdPaciente, r.NombrePaciente, r.ApellidoPaciente })
                    .Select(g => new
                    {
                        g.Key.IdPaciente,
                        g.Key.NombrePaciente,
                        g.Key.ApellidoPaciente,
                        HistorialClinico = g.Select(hc => new
                        {
                            hc.Id,
                            hc.IdCita,
                            hc.FechaInicioCita,
                            hc.FechaFinCita,
                            hc.EstatusCita,
                            hc.Observaciones,
                            hc.Diagnostico,
                            hc.Padecimientos,
                            hc.Intervenciones,
                            hc.Fecha,
                            hc.PesoPaciente,
                            hc.TallaPaciente,
                            hc.GlucosaPaciente,
                            hc.OxigenacionPaciente,
                            hc.PresionPaciente,
                            hc.TemperaturaCorporalPaciente,
                            hc.IdMedico,
                            hc.NombreMedico,
                            hc.ApellidoMedico
                        }).ToList()
                    }).ToListAsync();

                // Validamos si tiene historial, sino retornamos un mensaje
                if (listaAgrupada.Count == 0)
                {
                    return Ok(new { message = "El paciente no cuenta con historial clínico" });
                }

                return Ok(listaAgrupada);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        //Obtener listado de historial clinico por médico --------------------------------------------------------
        [HttpGet]
        [Route("ObtenerHistorialClinicoPorMedico/{idMedico}")]
        public async Task<IActionResult> ObtenerHistorialMedico(int idMedico)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound($"El médico ingresado no existe.");
                }

                //Hacemos la consulta y formateamos la respuesta
                var listaAgrupada = await (
                    from hc in _baseDatos.HistorialClinico
                    join c in _baseDatos.Cita on hc.IdCita equals c.Id
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    where c.IdMedico == idMedico
                    select new
                    {
                        Id = hc.Id,
                        IdCita = hc.IdCita,
                        FechaInicioCita = c.FechaInicio,
                        FechaFinCita = c.FechaFin,
                        EstatusCita = c.Estatus,
                        Observaciones = hc.Observaciones,
                        Diagnostico = hc.Diagnostico,
                        Padecimientos = hc.Padecimientos,
                        Intervenciones = hc.Intervenciones,
                        Fecha = hc.Fecha,
                        PesoPaciente = hc.PesoPaciente,
                        TallaPaciente = hc.TallaPaciente,
                        GlucosaPaciente = hc.GlucosaPaciente,
                        OxigenacionPaciente = hc.OxigenacionPaciente,
                        PresionPaciente = hc.PresionPaciente,
                        TemperaturaCorporalPaciente = hc.TemperaturaCorporalPaciente,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        IdMedico = c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido
                    })
                    .GroupBy(r => new { r.IdMedico, r.NombreMedico, r.ApellidoMedico })
                    .Select(g => new
                    {
                        g.Key.IdMedico,
                        g.Key.NombreMedico,
                        g.Key.ApellidoMedico,
                        HistorialClinico = g.Select(hc => new
                        {
                            hc.Id,
                            hc.IdCita,
                            hc.FechaInicioCita,
                            hc.FechaFinCita,
                            hc.EstatusCita,
                            hc.Observaciones,
                            hc.Diagnostico,
                            hc.Padecimientos,
                            hc.Intervenciones,
                            hc.Fecha,
                            hc.PesoPaciente,
                            hc.TallaPaciente,
                            hc.GlucosaPaciente,
                            hc.OxigenacionPaciente,
                            hc.PresionPaciente,
                            hc.TemperaturaCorporalPaciente,
                            hc.IdPaciente,
                            hc.NombrePaciente,
                            hc.ApellidoPaciente
                        }).ToList()
                    }).ToListAsync();

                // Validamos si tiene historial sino retornamos un mensaje
                if (listaAgrupada.Count == 0)
                {
                    return Ok(new { message = "El médico no cuenta con registros de historial clínico" });
                }

                return Ok(listaAgrupada);


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Obtener detalles de un de historial clinico--------------------------------------------------------
        [HttpGet]
        [Route("DetallesHistorialClinico/{id}")]
        public async Task<IActionResult> DetallesHistorialMedico(int id)
        {
            try
            {
                //Hacemos la consulta y formateamos la respuesta
                var detallesHistorial = await (
                    from hc in _baseDatos.HistorialClinico
                    join c in _baseDatos.Cita on hc.IdCita equals c.Id
                    join p in _baseDatos.Paciente on c.IdPaciente equals p.Id
                    join m in _baseDatos.Medicos on c.IdMedico equals m.Id
                    where hc.Id == id
                    select new
                    {
                        Id = hc.Id,
                        IdCita = hc.IdCita,
                        FechaInicioCita = c.FechaInicio,
                        FechaFinCita = c.FechaFin,
                        EstatusCita = c.Estatus,
                        Observaciones = hc.Observaciones,
                        Diagnostico = hc.Diagnostico,
                        Padecimientos = hc.Padecimientos,
                        Intervenciones = hc.Intervenciones,
                        Fecha = hc.Fecha,
                        PesoPaciente = hc.PesoPaciente,
                        TallaPaciente = hc.TallaPaciente,
                        GlucosaPaciente = hc.GlucosaPaciente,
                        OxigenacionPaciente = hc.OxigenacionPaciente,
                        PresionPaciente = hc.PresionPaciente,
                        TemperaturaCorporalPaciente = hc.TemperaturaCorporalPaciente,
                        IdPaciente = c.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido,
                        IdMedico = c.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido
                    }).ToListAsync();

                //Validamos si tiene hostorial sino retornamos un mensaje
                if (detallesHistorial.Count == 0)
                {
                    return NotFound(new { message = "No se tienen registros del historial clínico recibido." });
                }

                return Ok(detallesHistorial);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
