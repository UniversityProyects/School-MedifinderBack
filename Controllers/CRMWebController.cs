using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarAdministrador;
using static MediFinder_Backend.ModelosEspeciales.CRMWeb;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMWebController : Controller
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        //Contructor del controlador
        public CRMWebController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }
        
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginAdmonDTO loginAdmonDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "Datos inválidos. Por favor, verifica los campos ingresados.",
                    datos = (object)null
                });
            }

            try
            {
                var administrador = await _baseDatos.Administrador
                    .FirstOrDefaultAsync(a => a.Email == loginAdmonDTO.Email && a.Contrasena == loginAdmonDTO.Contrasena);

                if (administrador == null)
                {
                    return Unauthorized(new
                    {
                        estatus = "error",
                        mensaje = "Credenciales incorrectas. Por favor, verifique su correo electrónico y contraseña.",
                        datos = (object)null
                    });
                }
                else {
                    // Verificar si el estatus del usuario es 0 (aún no validado)
                    if (administrador.Estatus == "0")
                    {
                        return BadRequest(new
                        {
                            CodigoError = 400,
                            estatus = "error",
                            mensaje = "El usuario aún no ha sido validado. Por favor, contacte al administrador.",
                            datos = (object)null
                        });
                    }

                    var esAdminMaestro = await _baseDatos.AdministradorRol
                    .Where(rol => rol.IdAdministrador == administrador.Id)
                    .Select(rol => rol.EsAdminMaestro == 1)
                    .FirstOrDefaultAsync();

                    var isAdminMaestro = esAdminMaestro ? "Admin Maestro" : "Admin Normal";

                    var data = new
                    {
                        id = administrador.Id,
                        email = administrador.Email,
                        nombreCompleto = $"{administrador.Nombre} {administrador.Apellido}",
                        estatus = administrador.Estatus,
                        esAdminMaestro = isAdminMaestro
                    };

                    return Ok(new
                    {
                        estatus = "success",
                        mensaje = "El usuario accedió correctamente.",
                        datos = data
                    });
                }
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }


        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarAdministrador([FromBody] AdministradorDTO administradorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "Datos inválidos. Por favor, verifica los campos ingresados.",
                    datos = (object)null
                });
            }
            try
            {
                if (await ExisteAdministrador(administradorDTO.Nombre, administradorDTO.Apellido, administradorDTO.Email))
                {
                    return BadRequest(new
                    {
                        estatus = "error",
                        mensaje = "Ya existe un administrador con el mismo nombre o correo electrónico.",
                        datos = (object)null
                    });
                }
                // Crea un nuevo administrador
                var administradorNuevo = new Administrador
                {
                    Nombre = administradorDTO.Nombre,
                    Apellido = administradorDTO.Apellido,
                    Email = administradorDTO.Email,
                    Contrasena = administradorDTO.Contrasena,
                    Estatus = "0" 
                };
                // Guardar el administrador en la base de datos
                _baseDatos.Administrador.Add(administradorNuevo);
                await _baseDatos.SaveChangesAsync();
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Administrador registrado correctamente",
                    datos = new { id = administradorNuevo.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }


        [HttpPut]
        [Route("ActivarAdministrador/{id}")]
        public async Task<IActionResult> ActivarAdministrador(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "El ID proporcionado no es válido.",
                    datos = (object)null
                });
            }

            try
            {
                var administrador = await _baseDatos.Administrador
                .FirstOrDefaultAsync(m => (m.Id == id) );
                if (administrador == null)
                {
                    return NotFound(new
                    {
                        estatus = "error",
                        mensaje = "Administrador no encontrado.",
                        datos = (object)null
                    });
                }

                // Actualizar el estatus del administrador a '1' (activo)
                administrador.Estatus = "1";
                _baseDatos.Administrador.Update(administrador);
                await _baseDatos.SaveChangesAsync();

                var fechaActual = DateTime.Now.ToString("yyyy-MM-dd"); 
                var horaActual = DateTime.Now.ToString("HH:mm:ss"); 

                var nuevoRegistro = new AdministradorMedicoAutorizado
                {
                    IdAdministrador = id,
                    IdMedico = null,
                    IdAutoriza = id,
                    FechaModificacion = DateOnly.FromDateTime(DateTime.Now),
                    HoraModifica = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay),
                    Estatus = "1"
                };
                _baseDatos.AdministradorMedicoAutorizado.Add(nuevoRegistro);
                await _baseDatos.SaveChangesAsync();
                
                return Ok(new
                {
                    estatus = "success",
                    mensaje = $"El estatus del administrador {administrador.Nombre} {administrador.Apellido} ha sido actualizado.",
                    datos = new { id = administrador.Id, estatus = administrador.Estatus }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        [HttpPut]
        [Route("DesactivarAdministrador/{id}")]
        public async Task<IActionResult> DesactivarAdministrador(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "El ID proporcionado no es válido.",
                    datos = (object)null
                });
            }

            try
            {
                var administrador = await _baseDatos.Administrador
                .FirstOrDefaultAsync(m => (m.Id == id) );

                if (administrador == null)
                {
                    return NotFound(new
                    {
                        estatus = "error",
                        mensaje = "Administrador no encontrado.",
                        datos = (object)null
                    });
                }

                // Actualizar el estatus del administrador a '0' (Inactivo)
                administrador.Estatus = "0";
                _baseDatos.Administrador.Update(administrador);
                await _baseDatos.SaveChangesAsync();

                var fechaActual = DateTime.Now.ToString("yyyy-MM-dd");
                var horaActual = DateTime.Now.ToString("HH:mm:ss");

                var nuevoRegistro = new AdministradorMedicoAutorizado
                {
                    IdAdministrador = id,
                    IdMedico = null,
                    IdAutoriza = id,
                    FechaModificacion = DateOnly.FromDateTime(DateTime.Now),
                    HoraModifica = TimeOnly.FromTimeSpan(DateTime.Now.TimeOfDay),
                    Estatus = "0"
                };
                _baseDatos.AdministradorMedicoAutorizado.Add(nuevoRegistro);
                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    estatus = "success",
                    mensaje = $"El estatus del administrador {administrador.Nombre} {administrador.Apellido} ha sido actualizado.",
                    datos = new { id = administrador.Id, estatus = administrador.Estatus }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        [HttpGet]
        [Route("ObtenerAdministradores")]
        public async Task<IActionResult> ObtenerAdministradores()
        {
            try
            {
                // Realizamos la consulta con un LEFT JOIN para incluir administradores sin rol
                var administradores = await (from admin in _baseDatos.Administrador
                                             join rol in _baseDatos.AdministradorRol
                                             on admin.Id equals rol.IdAdministrador into adminRol
                                             from ar in adminRol.DefaultIfEmpty()  // LEFT JOIN
                                             select new
                                             {
                                                 Id = admin.Id,
                                                 nombre = admin.Nombre,
                                                 apellidos = admin.Apellido,
                                                 correo = admin.Email,
                                                 contrasenia = admin.Contrasena,
                                                 estatus = admin.Estatus,
                                                 estatusText = admin.Estatus == "1" ? "Activo" : "Inactivo",
                                                 // Si rol es null, asignamos "Admin Normal", si no, asignamos "Admin Maestro"
                                                 rolTipo = ar != null && ar.EsAdminMaestro == 1 ? "Admin Maestro" : "Admin Normal"
                                             }).ToListAsync();

                // Verificamos si no se encuentran administradores
                if (administradores == null || !administradores.Any())
                {
                    return NotFound(new
                    {
                        estatus = "error",
                        mensaje = "No se encontraron administradores.",
                        datos = (object)null
                    });
                }
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Administradores obtenidos correctamente.",
                    datos = administradores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        //MÉTODOS PARA HISTORIAL DE CONTACTO ----------------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("BuscarMedicos")]
        public async Task<IActionResult> BuscarMedicos([FromQuery] string termino)
        {
            if (string.IsNullOrWhiteSpace(termino) || termino.Length < 5)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "El término de búsqueda debe tener al menos 5 caracteres.",
                    datos = (object)null
                });
            }
            try
            {
                var medicosEncontrados = await _baseDatos.Medicos
                    .Where(m => m.Nombre.Contains(termino) || m.Apellido.Contains(termino))
                    .Take(5)
                    .Select(m => new
                    {
                        m.Id,
                        NombreCompleto = m.Nombre + " " + m.Apellido  // Concatenamos el nombre y apellido
                    })
                    .ToListAsync();
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Médicos encontrados correctamente.",
                    datos = medicosEncontrados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }


        [HttpGet]
        [Route("ObtenerHistoriales")]
        public async Task<IActionResult> ObtenerHistoriales()
        {
            try
            {
                var historiales = await (from h in _baseDatos.Historial
                                         join m in _baseDatos.Medicos on h.IdMedico equals m.Id
                                         join a in _baseDatos.Administrador on h.IdAdministrador equals a.Id
                                         join mc in _baseDatos.MedioContactos on h.IdMedioContacto equals mc.IdMedioContacto
                                         select new
                                         {
                                             Id = h.IdHistorial,
                                             MedicoNombre = m.Nombre + " " + m.Apellido,   
                                             AdministradorNombre = a.Nombre + " " + a.Apellido, 
                                             Fecha = h.Fecha,
                                             Observacion = h.Observaciones,
                                             MedioContacto = mc.Descripcion,
                                             NivelSatisfaccion = h.NivelSatisfaccion
                                         }).ToListAsync();

                if (historiales == null || !historiales.Any())
                {
                    return NotFound(new
                    {
                        estatus = "error",
                        mensaje = "No se encontraron historiales.",
                        datos = (object)null
                    });
                }
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Historiales obtenidos correctamente.",
                    datos = historiales
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }


        [HttpPost]
        [Route("AgregarHistorial")]
        public async Task<IActionResult> AgregarHistorial([FromBody] HistorialDTO historialDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "Datos inválidos. Por favor, verifica los campos ingresados.",
                    datos = (object)null
                });
            }
            try
            {
                var historialNuevo = new Historial
                {
                    IdMedico = historialDTO.IdMedico,
                    IdAdministrador = historialDTO.IdAdministrador,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Observaciones = historialDTO.Observaciones,
                    IdMedioContacto = historialDTO.IdMedioContacto,
                    NivelSatisfaccion = historialDTO.NivelSatisfaccion
                };
                _baseDatos.Historial.Add(historialNuevo);
                await _baseDatos.SaveChangesAsync();
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Historial agregado correctamente.",
                    datos = new { id = historialNuevo.IdHistorial }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        [HttpPut]
        [Route("ModificarHistorial/{id}")]
        public async Task<IActionResult> ModificarHistorial(int id, [FromBody] HistorialDTO historialDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "Datos inválidos. Por favor, verifica los campos ingresados.",
                    datos = (object)null
                });
            }
            try
            {
                var historialExistente = await _baseDatos.Historial.FindAsync(id);
                if (historialExistente == null)
                {
                    return NotFound(new
                    {
                        estatus = "error",
                        mensaje = "Historial no encontrado.",
                        datos = (object)null
                    });
                }
                historialExistente.Observaciones = historialDTO.Observaciones;
                historialExistente.NivelSatisfaccion = historialDTO.NivelSatisfaccion;
                historialExistente.IdMedico = historialDTO.IdMedico;
                historialExistente.IdAdministrador = historialDTO.IdAdministrador;
                historialExistente.IdMedioContacto = historialDTO.IdMedioContacto;
                
                _baseDatos.Historial.Update(historialExistente);
                await _baseDatos.SaveChangesAsync();
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Historial actualizado correctamente.",
                    datos = new { id = historialExistente.IdHistorial }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        [HttpDelete]
        [Route("EliminarHistorial/{id}")]
        public async Task<IActionResult> EliminarHistorial(int id)
        {
            try
            {
                var historialExistente = await _baseDatos.Historial.FindAsync(id);
                if (historialExistente == null)
                {
                    return NotFound(new
                    {
                        estatus = "error",
                        mensaje = "Historial no encontrado.",
                        datos = (object)null
                    });
                }
                _baseDatos.Historial.Remove(historialExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Historial eliminado correctamente.",
                    datos = new { id = historialExistente.IdHistorial }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }



        //MÉTODOS PARA TIPOS DE MEDIOS DE CONTACTO -------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("ConsultarMediosContacto")]
        public async Task<IActionResult> ConsultarMediosContacto()
        {
            try
            {
                var mediosContacto = await _baseDatos.MedioContactos
                    .Select(m => new
                    {
                        m.IdMedioContacto,
                        m.Descripcion
                    })
                    .ToListAsync();

                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Medios de contacto obtenidos correctamente.",
                    datos = mediosContacto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        [HttpPost]
        [Route("AgregarMedioContacto")]
        public async Task<IActionResult> AgregarMedioContacto([FromBody] string descripcion)
        {
            if (string.IsNullOrEmpty(descripcion))
            {
                return BadRequest(new
                {
                    estatus = "error",
                    mensaje = "La descripción es obligatoria.",
                    datos = (object)null
                });
            }
            try
            {
                var medioContactoNuevo = new MedioContacto
                {
                    Descripcion = descripcion
                };
                _baseDatos.MedioContactos.Add(medioContactoNuevo);
                await _baseDatos.SaveChangesAsync();
                return Ok(new
                {
                    estatus = "success",
                    mensaje = "Medio de contacto agregado correctamente.",
                    datos = new { id = medioContactoNuevo.IdMedioContacto }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    estatus = "error",
                    mensaje = $"Error interno del servidor: {ex.Message}",
                    datos = (object)null
                });
            }
        }

        //Metodo para verificar si no hay un administrador con la informacion ta registrada en la bd
        private async Task<System.Boolean> ExisteAdministrador(string nombre, string apellido, string email)
        {
            var medico = await _baseDatos.Administrador
                .FirstOrDefaultAsync(m => (m.Nombre == nombre && m.Apellido == apellido) || m.Email == email);

            if (medico != null)
            {
                return true;

            }
            return false;
        }

        // NUMERO DE MEDICOS Y PACIENTES --------------------------------------------------------
        [HttpGet]
        [Route("ContarMedicosYPacientes")]
        public async Task<IActionResult> ContarMedicosYPacientes()
        {
            try
            {
                // Definimos un diccionario para mapear el estatus a su tipo
                var estatusTipos = new Dictionary<string, string>
        {
            { "1", "Registro nuevo" },
            { "2", "Sin Suscripción" },
            { "3", "Activo" },
            { "4", "Baja" }
        };

                // Contar médicos por estatus
                var medicosPorEstatus = await _baseDatos.Medicos
                    .GroupBy(m => m.Estatus)
                    .Select(g => new
                    {
                        Estatus = g.Key,
                        Tipo = estatusTipos[g.Key], // Asignar el tipo basado en el diccionario
                        Cantidad = g.Count()
                    }).ToListAsync();

                // Contar total de pacientes
                var totalPacientes = await _baseDatos.Paciente.CountAsync();

                // Formatear la respuesta
                var resultado = new
                {
                    MedicosPorEstatus = medicosPorEstatus,
                    TotalPacientes = totalPacientes
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("ObtenerListadoPacientes")]
        public async Task<IActionResult> ObtenerPacientes()
        {
            try
            {
                // Obtener todos los pacientes sin aplicar ningún filtro
                var pacientes = await _baseDatos.Paciente
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.Apellido,
                        p.Email,
                        p.Telefono,
                        p.FechaNacimiento,
                        p.Sexo,
                        p.Estatus
                    }).ToListAsync();

                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }



    }
}
