using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarHistorialClinico;
using static MediFinder_Backend.ModelosEspeciales.RegistrarPacientesAsignados;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesAsignadosController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public PacientesAsignadosController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Asignar Pacientes -------------------------------------------------
        [HttpPost]
        [Route("Asignar")]
        public async Task<IActionResult> AsignarPaciente([FromBody] AsignarPacienteDTO asignarPacienteDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == asignarPacienteDTO.IdMedico);
                if (existeMedico == null)
                {
                    return NotFound($"El médico ingresado no existe.");
                }

                //Validar que el Id del paciente recibido si existe en la BD
                var existePaciente = await _baseDatos.Paciente.FirstOrDefaultAsync(e => e.Id == asignarPacienteDTO.IdPaciente);
                if (existePaciente == null)
                {
                    return NotFound($"El paciente ingresado no existe.");
                }

                //Validar que no este esa asignación en la base de datos
                var existeAsignacion = await _baseDatos.PacientesAsignados
                    .FirstOrDefaultAsync(pa => pa.IdMedico == asignarPacienteDTO.IdMedico 
                    && pa.IdPaciente == asignarPacienteDTO.IdPaciente && pa.Estatus == "1");

                if (existeAsignacion != null)
                {
                    return BadRequest($"La asignación ingresada ya se encuentra registrada.");
                }

                //Formatear los valores en el modelo
                var asignacionNueva = new PacientesAsignado
                {
                    IdMedico = asignarPacienteDTO.IdMedico,
                    IdPaciente = asignarPacienteDTO.IdPaciente,
                    Fecha = asignarPacienteDTO.Fecha,
                    Estatus = "1",
                    FechaFinalizacion = null
                };

                // Guardar la asignacion en la base de datos
                _baseDatos.PacientesAsignados.Add(asignacionNueva);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Asignación registrada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        //Desasignar Pacientes ----------------------------------------------
        [HttpPut]
        [Route("Desasignar/{id}")]
        public async Task<IActionResult> DesasignarPacientes(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);

            }

            try
            {
                //Validar que este esa asignación en la base de datos
                var existeAsignacion = await _baseDatos.PacientesAsignados
                    .FirstOrDefaultAsync(pa => pa.Id == id);

                //Validamos si existe
                if (existeAsignacion == null )
                {
                    return NotFound($"No se tiene ningún registro de la asignación ingresada.");
                }
                //Validamos que no este como dada de baja
                if(existeAsignacion.Estatus == "0")
                {
                    return BadRequest($"La asignación ya se encuentra dada de baja.");
                }

                //Formateamos el estatus y la fecha
                existeAsignacion.Estatus = "0";
                existeAsignacion.FechaFinalizacion = DateTime.Now;

                //Guardamos la informacion nueva
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "La asignación ha sido dada de baja exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Listado de pacientes asignados (Perspectiva Medico) ---------------
        [HttpGet]
        [Route("ObtenerListadoPacientesAsignados/{idMedico}")]
        public async Task<IActionResult> ObtenerPacientesAsignados(int idMedico)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existeMedico = await _baseDatos.Medicos.FirstOrDefaultAsync(e => e.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound($"El médico ingresado no existe.");
                }

                //Ejecutamos la consulta para obtener los registros de la bd
                var resultado = from pa in _baseDatos.PacientesAsignados
                                join m in _baseDatos.Medicos on pa.IdMedico equals m.Id
                                join p in _baseDatos.Paciente on pa.IdPaciente equals p.Id
                                where pa.IdMedico == idMedico && pa.Estatus == "1"
                                select new
                                {
                                    pa.Id,
                                    pa.Fecha,
                                    pa.Estatus,
                                    pa.FechaFinalizacion,
                                    pa.IdMedico,
                                    NombreMedico = m.Nombre,
                                    ApellidoMedico = m.Apellido,
                                    pa.IdPaciente,
                                    NombrePaciente = p.Nombre,
                                    ApellidoPaciente = p.Apellido
                                };

                //Formateamos el JSON
                var listaAgrupada = await resultado
                    .GroupBy(r => new { r.IdMedico, r.NombreMedico, r.ApellidoMedico })
                    .Select(g => new
                    {
                        g.Key.IdMedico,
                        g.Key.NombreMedico,
                        g.Key.ApellidoMedico,
                        PacientesAsignados = g.Select(pa => new
                        {
                            pa.Id,
                            pa.Fecha,
                            pa.Estatus,
                            pa.FechaFinalizacion,
                            pa.IdPaciente,
                            pa.NombrePaciente,
                            pa.ApellidoPaciente
                        }).ToList()
                    }).ToListAsync();

                //Validamos si esta vacia
                if (listaAgrupada.Count == 0)
                {
                    return NotFound($"El médico ingresado no cuenta con ningún paciente asignado.");
                }

                return Ok(listaAgrupada);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        //Listado de médicos con asignacion (Perspectiva Paciente) ----------
        [HttpGet]
        [Route("ObtenerListadoMedicosPermitidos/{idPaciente}")]
        public async Task<IActionResult> ObtenerMedicosPermitidos(int idPaciente)
        {
            try
            {
                //Validar que el Id del médico recibido si existe en la BD
                var existePaciente = await _baseDatos.Paciente.FirstOrDefaultAsync(e => e.Id == idPaciente);
                if (existePaciente == null)
                {
                    return NotFound($"El paciente ingresado no existe.");
                }

                //Consulta para sacar todos los medicos permitidos
                var queryPacientesAsignados = from pa in _baseDatos.PacientesAsignados
                                join m in _baseDatos.Medicos on pa.IdMedico equals m.Id
                                join p in _baseDatos.Paciente on pa.IdPaciente equals p.Id
                                where pa.IdPaciente == idPaciente && pa.Estatus == "1"
                                select new
                                {
                                    pa.Id,
                                    pa.Fecha,
                                    pa.Estatus,
                                    pa.FechaFinalizacion,
                                    pa.IdMedico,
                                    NombreMedico = m.Nombre,
                                    ApellidoMedico = m.Apellido,
                                    pa.IdPaciente,
                                    NombrePaciente = p.Nombre,
                                    ApellidoPaciente = p.Apellido
                                };

                //Formateamos la respuesta para estilizar el formato del json
                var listaAgrupada = await queryPacientesAsignados
                    .GroupBy(r => new { r.IdPaciente, r.NombrePaciente, r.ApellidoPaciente })
                    .Select(g => new
                    {
                        g.Key.IdPaciente,
                        g.Key.NombrePaciente,
                        g.Key.ApellidoPaciente,
                        MedicosPermitidos = g.Select(pa => new
                        {
                            pa.Id,
                            pa.Fecha,
                            pa.Estatus,
                            pa.FechaFinalizacion,
                            pa.IdMedico,
                            pa.NombreMedico,
                            pa.ApellidoMedico
                        }).ToList()
                    }).ToListAsync();

                //Validamos si esta vacia
                if (listaAgrupada.Count == 0)
                {
                    return NotFound($"El paciente ingresado no cuenta con médicos permitidos para revisar su historial");
                }

                return Ok(listaAgrupada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Detalles de una asignación -----------------------------------------------------
        [HttpGet]
        [Route("DetallesAsignacion/{id}")]
        public async Task<IActionResult> DetallesAsignacion(int id)
        {
            try
            {
                //Consulta para sacar todos los medicos permitidos
                var asignacion = await (
                    from pa in _baseDatos.PacientesAsignados
                    join m in _baseDatos.Medicos on pa.IdMedico equals m.Id
                    join p in _baseDatos.Paciente on pa.IdPaciente equals p.Id
                    where pa.Id == id
                    select new
                    {
                        pa.Id,
                        pa.Fecha,
                        pa.Estatus,
                        pa.FechaFinalizacion,
                        pa.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido,
                        pa.IdPaciente,
                        NombrePaciente = p.Nombre,
                        ApellidoPaciente = p.Apellido
                    }
                ).ToListAsync();


                //Validamos si esta vacia
                if (asignacion.Count == 0)
                {
                    return NotFound($"No se encontró ningún registro de la asignación ingresada.");
                }

                return Ok(asignacion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}
