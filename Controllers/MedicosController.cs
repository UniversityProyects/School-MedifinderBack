﻿using MediFinder_Backend.Models;
using MediFinder_Backend.ModelosEspeciales;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using static MediFinder_Backend.ModelosEspeciales.RegistrarMedico;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicosController : Controller
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public MedicosController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        // Registrar Médicos --------------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] MedicoDTO medicoDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                // Verificar si todas las especialidades existen
                foreach (var especialidadDTO in medicoDTO.Especialidades)
                {
                    var especialidadExistente = await _baseDatos.Especialidad
                        .FirstOrDefaultAsync(e => e.Id == especialidadDTO.Id_Especialidad);

                    if (especialidadExistente == null)
                    {
                        return BadRequest($"Una de las especialidades no existe. El médico no ha sido registrado.");
                    }
                }

                //Permite validar si ya existe una cuenta registrada.
                if (await ExisteMedico(medicoDTO.Nombre, medicoDTO.Apellido, medicoDTO.Email))
                {
                    return BadRequest($"Ya existe un médico con el mismo nombre o correo electrónico.");
                }


                var medico = new Medico
                {
                    Nombre = medicoDTO.Nombre,
                    Apellido = medicoDTO.Apellido,
                    Email = medicoDTO.Email,
                    Contrasena = medicoDTO.Contrasena,
                    Telefono = medicoDTO.Telefono,
                    Calle = medicoDTO.Calle,
                    Colonia = medicoDTO.Colonia,
                    Numero = medicoDTO.Numero,
                    Ciudad = medicoDTO.Ciudad,
                    Pais = medicoDTO.Pais,
                    CodigoPostal = medicoDTO.Codigo_Postal,
                    Estatus = "1", // Nuevo usuario Sin Validar
                    FechaRegistro = DateTime.Now 
                };

                // Guardar el médico en la base de datos
                _baseDatos.Medicos.Add(medico);
                await _baseDatos.SaveChangesAsync();

                // Permite leer las especialidades
                foreach (var especialidadDTO in medicoDTO.Especialidades)
                {
                    var especialidadMedico = new EspecialidadMedicoIntermedium
                    {
                        IdEspecialidad = especialidadDTO.Id_Especialidad,
                        IdMedico = medico.Id,
                        NumCedula = especialidadDTO.Num_Cedula,
                        Honorarios = especialidadDTO.Honorarios
                    };

                    // Guardar la especialidad_medico en la base de datos
                    _baseDatos.EspecialidadMedicoIntermedia.Add(especialidadMedico);
                }

                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Médico registrado correctamente", medico.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Verificar Login Médico -----------------------------------------------------------------------------------------------------------

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var medico = await _baseDatos.Medicos
                    .Include(m => m.EspecialidadMedicoIntermedia)
                    .ThenInclude(em => em.IdEspecialidadNavigation)
                    .FirstOrDefaultAsync(m => m.Email == loginDTO.Email && m.Contrasena == loginDTO.Contrasena);

                if (medico == null)
                {
                    return NotFound("Correo electrónico o contraseña incorrectos.");
                }

                switch (int.Parse(medico.Estatus))
                {
                    case 1: // Nuevo/Sin Validar
                        return BadRequest("El usuario está pendiente de validación.");
                    case 2: // Activo/Validado
                        return BadRequest("El usuario está pendiente de pago de suscripción.");
                    case 3: // Activo/Pago Realizado
                        var medicoDTO = new
                        {
                            NombreCompleto = $"{medico.Nombre} {medico.Apellido}",
                            Especialidades = medico.EspecialidadMedicoIntermedia.Select(em => new
                            {
                                Especialidad = em.IdEspecialidadNavigation.Nombre,
                                Honorarios = em.Honorarios
                            }),
                            Direccion = $"{medico.Calle}, {medico.Colonia}, {medico.Numero}, {medico.Ciudad}, {medico.Pais}, {medico.CodigoPostal}",
                            Telefono = medico.Telefono
                        };
                        return Ok(medicoDTO);
                    case 4: // Inactivo
                        return BadRequest("El usuario fue dado de baja.");
                    default:
                        return BadRequest("Ocurrió un error desconocido.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }



        // Modificar Médico y Especialidades -------------------------------------------------------------------------------------------------
        [HttpPut]
        [Route("ModificarMedico/{idMedico}")]
        public async Task<IActionResult> ModificarMedico(int idMedico, [FromBody] MedicoDTO medicoDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verificar si el médico existe
                var medicoExistente = await _baseDatos.Medicos
                    .Include(m => m.EspecialidadMedicoIntermedia)
                    .ThenInclude(em => em.IdEspecialidadNavigation)
                    .FirstOrDefaultAsync(m => m.Id == idMedico);

                if (medicoExistente == null)
                {
                    return NotFound($"No existe ningún médico con el Id {idMedico}");
                }

                // Verificar si todas las especialidades existen
                foreach (var especialidadDTO in medicoDTO.Especialidades)
                {
                    var especialidadExistente = await _baseDatos.Especialidad
                        .FirstOrDefaultAsync(e => e.Id == especialidadDTO.Id_Especialidad);

                    if (especialidadExistente == null)
                    {
                        return BadRequest($"Error, una de las especialidades no existe.");
                    }
                }


                // Actualizar datos del médico
                medicoExistente.Nombre = medicoDTO.Nombre;
                medicoExistente.Apellido = medicoDTO.Apellido;
                medicoExistente.Email = medicoDTO.Email;
                medicoExistente.Contrasena = medicoDTO.Contrasena;
                medicoExistente.Telefono = medicoDTO.Telefono;
                medicoExistente.Calle = medicoDTO.Calle;
                medicoExistente.Colonia = medicoDTO.Colonia;
                medicoExistente.Numero = medicoDTO.Numero;
                medicoExistente.Ciudad = medicoDTO.Ciudad;
                medicoExistente.Pais = medicoDTO.Pais;
                medicoExistente.CodigoPostal = medicoDTO.Codigo_Postal;

                // Actualizar especialidades del médico
                foreach (var especialidadDTO in medicoDTO.Especialidades)
                {
                    // Buscar si el médico ya tiene esta especialidad
                    var especialidadMedico = medicoExistente.EspecialidadMedicoIntermedia
                        .FirstOrDefault(em => em.IdEspecialidad == especialidadDTO.Id_Especialidad);

                    if (especialidadMedico != null)
                    {
                        // Si existe, actualizar los datos
                        especialidadMedico.NumCedula = especialidadDTO.Num_Cedula;
                        especialidadMedico.Honorarios = especialidadDTO.Honorarios;
                    }
                    else
                    {
                        // Si no existe, agregar nueva especialidad al médico
                        var nuevaEspecialidadMedico = new EspecialidadMedicoIntermedium
                        {
                            IdEspecialidad = especialidadDTO.Id_Especialidad,
                            NumCedula = especialidadDTO.Num_Cedula,
                            Honorarios = especialidadDTO.Honorarios
                        };
                        medicoExistente.EspecialidadMedicoIntermedia.Add(nuevaEspecialidadMedico);
                    }
                }

                // Eliminar especialidades que ya no están en el DTO
                var especialidadesAEliminar = medicoExistente.EspecialidadMedicoIntermedia
                    .Where(em => !medicoDTO.Especialidades.Any(dto => dto.Id_Especialidad == em.IdEspecialidad))
                    .ToList();

                foreach (var especialidadEliminar in especialidadesAEliminar)
                {
                    _baseDatos.EspecialidadMedicoIntermedia.Remove(especialidadEliminar);
                }

                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = $"Médico con Id {idMedico} modificado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }



        // Obtener Lista de Médicos Registrados ---------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("ObtenerMedicosRegistrados")]
        public async Task<IActionResult> ObtenerMedicos()
        {
            try
            {
                var listaMedicos = await _baseDatos.Medicos
                    .Include(m => m.EspecialidadMedicoIntermedia)
                    .ThenInclude(em => em.IdEspecialidadNavigation) 
                    .ToListAsync();

                var listaMedicosDTO = listaMedicos.Select(m => new
                {
                    m.Id,
                    m.Nombre,
                    m.Apellido,
                    m.Email,
                    m.Telefono,
                    m.Calle,
                    m.Colonia,
                    m.Numero,
                    m.Ciudad,
                    m.Pais,
                    m.CodigoPostal,
                    m.Estatus, 
                    FechaRegistro = m.FechaRegistro?.ToString("yyyy-MM-dd HH:mm:ss"), 
                    Especialidades = m.EspecialidadMedicoIntermedia.Select(em => new
                    {
                        em.IdEspecialidad,
                        em.NumCedula,
                        em.Honorarios,
                        Especialidad = em.IdEspecialidadNavigation?.Nombre 
                    })
                });

                return Ok(listaMedicosDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Obtener Lista de Médicos por Especialidad ---------------------------------------------------------------------------------------

        [HttpGet]
        [Route("ObtenerMedicosPorEspecialidad/{nombreEspecialidad}")]
        public async Task<IActionResult> ObtenerMedicosPorEspecialidad(string nombreEspecialidad)
        {
            try
            {
                // Obtener todos los médicos con sus especialidades relacionadas
                var listaMedicos = await _baseDatos.Medicos
                    .Include(m => m.EspecialidadMedicoIntermedia)
                    .ThenInclude(em => em.IdEspecialidadNavigation)
                    .ToListAsync();

                // Filtrar los médicos cuya especialidad contenga parcialmente el nombreEspecialidad
                var medicosFiltrados = listaMedicos.Where(m =>
                    m.EspecialidadMedicoIntermedia.Any(em =>
                        em.IdEspecialidadNavigation.Nombre.ToLower().Contains(nombreEspecialidad.ToLower())
                    )
                ).ToList();

                // Mapear la lista de médicos filtrados a un formato DTO para devolver
                var listaMedicosDTO = medicosFiltrados.Select(m => new
                {
                    IdDoctor = m.Id,
                    NombreCompleto = $"{m.Nombre} {m.Apellido}",
                    Especialidades = m.EspecialidadMedicoIntermedia
                                        .Where(em => em.IdEspecialidadNavigation.Nombre.ToLower().Contains(nombreEspecialidad.ToLower()))
                                        .Select(em => em.IdEspecialidadNavigation.Nombre),
                    Direccion = $"{m.Calle}, {m.Colonia}, {m.Numero}, {m.Ciudad}, {m.Pais}, {m.CodigoPostal}",
                    Honorarios = m.EspecialidadMedicoIntermedia
                                    .Where(em => em.IdEspecialidadNavigation.Nombre.ToLower().Contains(nombreEspecialidad.ToLower()))
                                    .Select(em => em.Honorarios)
                                    .FirstOrDefault()
                });

                return Ok(listaMedicosDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpGet]
        [Route("ObtenerHorasDeTrabajo/{idMedico}/{dia}/{fecha}")]
        public async Task<IActionResult> ObtenerHorasDeTrabajo(int idMedico, int dia, DateTime fecha)
        {
            try
            {
                // Validar que la fecha no sea inhábil para el médico
                var fechaInhabil = await _baseDatos.DiaInhabil
                .Where(d => d.IdMedico == idMedico && d.Fecha.HasValue && d.Fecha.Value.Date == fecha.Date)
                .AnyAsync();

                if (fechaInhabil)
                {
                    return BadRequest("El día solicitado es inhábil para el médico.");
                }

                // Verificar que el día esté dentro del rango válido
                if (dia < 1 || dia > 7)
                {
                    return BadRequest("El día debe estar entre 1 (lunes) y 7 (domingo).");
                }

                // Obtener los horarios del médico para el día solicitado
                var horarios = await _baseDatos.Horarios
                    .Where(h => h.IdMedico == idMedico && h.Dia == dia)
                    .ToListAsync();

                if (horarios == null || !horarios.Any())
                {
                    return NotFound("No se encontraron horarios para el médico en el día especificado.");
                }

                // Determinar todas las horas trabajadas
                var horasTrabajadas = new HashSet<string>();

                foreach (var horario in horarios)
                {
                    var inicio = horario.HoraInicio;
                    var fin = horario.HoraFin;

                    if (inicio == null || fin == null)
                        continue;

                    var horaInicio = inicio.Value;
                    var horaFin = fin.Value;

                    // Generar las horas en el rango de inicio a fin
                    for (var current = horaInicio; current <= horaFin; current = current.Add(TimeSpan.FromHours(1)))
                    {
                        horasTrabajadas.Add(current.ToString("HH:mm"));
                    }
                }

                // Ordenar las horas y convertir a una lista
                var horasOrdenadas = horasTrabajadas.OrderBy(h => h).ToList();

                return Ok(horasOrdenadas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("RegistrarHorasTrabajo")]
        public async Task<IActionResult> RegistrarHorasTrabajo([FromBody] RegistrarHorasDTO registrarHorasDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Validar si el médico existe
                var medicoExistente = await _baseDatos.Medicos.FindAsync(registrarHorasDTO.IdMedico);
                if (medicoExistente == null)
                {
                    return NotFound("El médico especificado no existe.");
                }

                // Convertir cadenas a TimeOnly
                if (!TryParseTimeOnly(registrarHorasDTO.HoraInicio, out var horaInicio) ||
                    !TryParseTimeOnly(registrarHorasDTO.HoraFin, out var horaFin))
                {
                    return BadRequest("Formato de hora inválido.");
                }

                // Buscar un registro existente con el mismo IdMedico y Dia
                var horarioExistente = await _baseDatos.Horarios
                    .FirstOrDefaultAsync(h => h.IdMedico == registrarHorasDTO.IdMedico && h.Dia == registrarHorasDTO.DiaSemana);

                if (horarioExistente != null)
                {
                    // Actualizar el registro existente
                    horarioExistente.HoraInicio = horaInicio;
                    horarioExistente.HoraFin = horaFin;

                    _baseDatos.Horarios.Update(horarioExistente);
                }
                else
                {
                    // Registrar la nueva hora de trabajo
                    var nuevoHorario = new Horario
                    {
                        IdMedico = registrarHorasDTO.IdMedico,
                        Dia = registrarHorasDTO.DiaSemana,
                        HoraInicio = horaInicio,
                        HoraFin = horaFin
                    };

                    _baseDatos.Horarios.Add(nuevoHorario);
                }

                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Horas de trabajo registradas correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Obtener detalles medico---------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("DetallesMedico/{id}")]
        public async Task<IActionResult> DetallesMedico(int id)
        {
            try
            {
                var resultado = from m in _baseDatos.Medicos
                                join emi in _baseDatos.EspecialidadMedicoIntermedia on m.Id equals emi.IdMedico
                                join e in _baseDatos.Especialidad on emi.IdEspecialidad equals e.Id
                                where m.Id == 1
                                group new { m, emi, e } by new
                                {
                                    m.Id,
                                    m.Nombre,
                                    m.Apellido,
                                    m.Email,
                                    m.Contrasena,
                                    m.Telefono,
                                    m.Calle,
                                    m.Colonia,
                                    m.Numero,
                                    m.Ciudad,
                                    m.Pais,
                                    m.CodigoPostal,
                                    m.Estatus,
                                    m.FechaRegistro,
                                    m.FechaValidacion,
                                    m.FechaBaja
                                } into grp
                                select new
                                {
                                    Id = grp.Key.Id,
                                    Nombre = grp.Key.Nombre,
                                    Apellido = grp.Key.Apellido,
                                    Email = grp.Key.Email,
                                    Contrasena = grp.Key.Contrasena,
                                    Telefono = grp.Key.Telefono,
                                    Calle = grp.Key.Calle,
                                    Colonia = grp.Key.Colonia,
                                    Numero = grp.Key.Numero,
                                    Ciudad = grp.Key.Ciudad,
                                    Pais = grp.Key.Pais,
                                    CodigoPostal = grp.Key.CodigoPostal,
                                    Estatus = grp.Key.Estatus,
                                    FechaRegistro = grp.Key.FechaRegistro,
                                    FechaValidacion = grp.Key.FechaValidacion,
                                    FechaBaja = grp.Key.FechaBaja,
                                    Especialidades = grp.Select(x => new
                                    {
                                        IdEspecialidad = x.emi.IdEspecialidad,
                                        NumCedula = x.emi.NumCedula,
                                        NombreEspecialidad = x.e.Nombre
                                    }).ToList()
                                };

                var listaResultados = await resultado.ToListAsync();

                // Validamos si la lista contiene algo
                if (listaResultados.Count == 0)
                {
                    return NotFound($"No se encontraron registros del médico.");
                }

                // Retornamos los resultados
                return Ok(listaResultados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }



        // Métodos para procesos de registro ************************************************************************************************
        private async Task<Boolean> ExisteMedico(string nombre, string apellido, string email)
        {
            var medico = await _baseDatos.Medicos
                .FirstOrDefaultAsync(m => (m.Nombre == nombre && m.Apellido == apellido) || m.Email == email);

            if (medico != null)
            {
                return true;
                
            }
            return false; 
        }

        private bool TryParseTimeOnly(string timeString, out TimeOnly timeOnly)
        {
            timeOnly = default;
            return TimeOnly.TryParse(timeString, out timeOnly);
        }




    }
}
