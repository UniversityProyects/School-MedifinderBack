using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarAdministrador;
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
                    where med.Estatus != "1"
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

        //Metodo para obtener la cantidad de suscripciones que tienen los medicos
        [HttpGet]
        [Route("ObtenerSuscripcionesMedicos")]
        public async Task<IActionResult> ObtenerSuscripcionesMedicos()
        {
            try
            {
                var resultados = from m in _baseDatos.Medicos
                        join s in _baseDatos.Suscripcion on m.Id equals s.IdMedico into suscripcionesGroup
                        from s in suscripcionesGroup.DefaultIfEmpty() // LEFT JOIN
                        where m.Estatus != "1"
                        group s by new { m.Id, m.Nombre, m.Apellido, m.Estatus } into grouped
                        select new
                        {
                            Id = grouped.Key.Id,
                            Nombre = grouped.Key.Nombre,
                            Apellido = grouped.Key.Apellido,
                            Estatus = grouped.Key.Estatus,
                            CantidadSuscripciones = grouped.Count(s => s != null) // Contar las suscripciones no nulas
                        };

                var listaResultado = await resultados.ToListAsync();



                return Ok(resultados);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Metodo para obtener las suscripciones realizadas por medico
        [HttpGet]
        [Route("ListadoSuscripcionesPorMedico/{id}")]
        public async Task<IActionResult> ListadoSuscripcionesPorMedico(int id)
        {
            try
            {
                var medicoExiste = await _baseDatos.Medicos.FindAsync(id);
                if (medicoExiste == null)
                {
                    return NotFound(new { CodigoHttp = 404, mensaje = $"Médico con id {id} no encontrado." });
                }

                //Consulta para sacar todos los medicos permitidos
                var resultado =
                        from s in _baseDatos.Suscripcion
                        join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                        join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                        join ps in _baseDatos.PagoSuscripcion on s.Id equals ps.IdSuscripcion into pagoSuscripcionesGroup
                        from ps in pagoSuscripcionesGroup.DefaultIfEmpty() // LEFT JOIN
                        where s.IdMedico == id
                        group new
                        {
                            s.Id,
                            s.IdTipoSuscripcion,
                            ts.Nombre,
                            ts.Descripcion,
                            ts.Precio,
                            EstatusPago = ps != null ? "PAGADA" : "NO PAGADA",
                            ps.FechaPago
                        }
                        by new
                        {
                            m.Id,
                            m.Nombre,
                            m.Apellido,
                            m.Estatus
                        } into grouped
                        select new
                        {
                            idMedico = grouped.Key.Id,
                            nombreMedico = grouped.Key.Nombre,
                            apellidoMedico = grouped.Key.Apellido,
                            estatusMedico = grouped.Key.Estatus,
                            suscripciones = grouped.Select(g => new
                            {
                                idSuscripcion = g.Id,
                                idTipoSuscripcion = g.IdTipoSuscripcion,
                                nombreTipoSuscripcion = g.Nombre,
                                descripcionTipoSuscripcion = g.Descripcion,
                                precioTipoSuscripcion = g.Precio,
                                estatusPago = g.EstatusPago,
                                fechaPago = g.FechaPago
                            }).ToList()
                        };

                var listaResultado = await resultado.ToListAsync();

                return Ok(listaResultado);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Metodo para detalle de una suscripcion
        [HttpGet]
        [Route("DetalleSuscripcionMedico/{id}")]
        public async Task<IActionResult> DetalleSuscripcionMedico(int id)
        {
            try
            {
                var suscripcionExiste = await _baseDatos.Suscripcion.FindAsync(id);
                if (suscripcionExiste == null)
                {
                    return NotFound(new { CodigoHttp = 404, mensaje = $"Suscripción con id {id} no encontrada." });
                }

                //Consulta para sacar todos los medicos permitidos
                var resultado =
                        from s in _baseDatos.Suscripcion
                        join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                        join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                        join ps in _baseDatos.PagoSuscripcion on s.Id equals ps.IdSuscripcion into pagoSuscripcionesGroup
                        from ps in pagoSuscripcionesGroup.DefaultIfEmpty() // LEFT JOIN
                        where s.Id == id
                        select new
                        {
                            IdSuscripcion = s.Id,
                            IdMedico = m.Id,
                            NombreMedico = m.Nombre,
                            ApellidoMedico = m.Apellido,
                            EstatusMedico = m.Estatus,
                            IdTipoSuscripcion = ts.Id,
                            NombreTipoSuscripcion = ts.Nombre,
                            DescripcionTipoSuscripcion = ts.Descripcion,
                            PrecioTipoSuscripcion = ts.Precio,
                            DuracionTipoSuscripcion = ts.Duracion,
                            FechaInicio = s.FechaInicio,
                            FechaFin = s.FechaFin,
                            EstatusPago = ps != null ? "PAGADA" : "NO PAGADA",
                            FechaPago = ps.FechaPago
                        };

                var listaResultado = await resultado.ToListAsync();


                return Ok(listaResultado);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Registrar Administrador ----------------------------------------------
        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarAdministrador([FromBody] AdministradorDTO administradorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { CodigoError = 400, Mensaje = "Datos inválidos. Por favor, verifica los campos ingresados." });
            }

            try
            {
                // Verifica si ya existe un administrador con el mismo nombre o correo electrónico.
                if (await ExisteAdministrador(administradorDTO.Nombre, administradorDTO.Apellido, administradorDTO.Email))
                {
                    return BadRequest(new { CodigoError = 400, Mensaje = "Ya existe un administrador con el mismo nombre o correo electrónico." });
                }

                // Crea un nuevo administrador
                var administradorNuevo = new Administrador
                {
                    Nombre = administradorDTO.Nombre.Trim(),
                    Apellido = administradorDTO.Apellido.Trim(),
                    Email = administradorDTO.Email.Trim(),
                    Contrasena = administradorDTO.Contrasena.Trim(),
                    Estatus = "0"
                };

                // Guardar el administrador en la base de datos
                _baseDatos.Administrador.Add(administradorNuevo);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { mensaje = "Administrador registrado correctamente", id = administradorNuevo.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoError = 500, Mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }


        // Verificar Login Administrador -----------------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginAdmonDTO loginAdmonDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { CodigoError = 400, Mensaje = "Datos inválidos. Por favor, verifica los campos ingresados." });
            }

            try
            {
                // Busca el administrador por correo electrónico y contraseña
                var administrador = await _baseDatos.Administrador
                    .FirstOrDefaultAsync(a => a.Email == loginAdmonDTO.Email && a.Contrasena == loginAdmonDTO.Contrasena);

                if (administrador == null)
                {
                    return Unauthorized(new { CodigoError = 401, mensaje = "Credenciales incorrectas. Por favor, verifique su correo electrónico y contraseña." });
                }

                if (administrador.Estatus == "0")
                {
                    return Unauthorized(new { CodigoError = 401, mensaje = "Usuario sin autorización" });
                }

                // Retornar los datos necesarios para el almacenamiento en localStorage
                return Ok(new
                {
                    email = administrador.Email,
                    nombreCompleto = $"{administrador.Nombre} {administrador.Apellido}",
                    id = administrador.Id,
                    estatus = administrador.Estatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoError = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }


        //Metodo para consultar la lista de administadores
        [HttpGet]
        [Route("ListadoAdministradores")]
        public async Task<IActionResult> ListadoAdministradores()
        {
            try
            {
                var administradores = _baseDatos.Administrador
                .Select(a => new
                {
                    a.Id,
                    a.Nombre,
                    a.Apellido,
                    a.Email,
                    a.Estatus
                })
                .OrderByDescending(a => a.Id)
                .ToList();

                return Ok(administradores);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Activar administrador  ----------------------------------------
        [HttpPut]
        [Route("ActivarAdministrador/{id}")]
        public async Task<IActionResult> ActivarAdministrador(int id)
        {

            var administradorExistente = await _baseDatos.Administrador.FindAsync(id);
            if (administradorExistente == null)
            {
                return NotFound(new { CodigoHttp = 404, mensaje = $"Administrador con id {id} no encontrado." });
            }

            try
            {
                administradorExistente.Estatus = "1";

                _baseDatos.Administrador.Update(administradorExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { CodigoHttp = 200, message = "Administrador activado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        //Desactivar administrador  ----------------------------------------
        [HttpPut]
        [Route("DesactivarAdministrador/{id}")]
        public async Task<IActionResult> DesactivarAdministrador(int id)
        {

            var administradorExistente = await _baseDatos.Administrador.FindAsync(id);
            if (administradorExistente == null)
            {
                return NotFound(new { CodigoHttp = 404, mensaje = $"Administrador con id {id} no encontrado." });
            }

            try
            {
                administradorExistente.Estatus = "0";

                _baseDatos.Administrador.Update(administradorExistente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { CodigoHttp = 200, message = "Administrador desactivado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
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
                    mensaje = "Se activo exitosamente.",
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

        [HttpGet]
        [Route("ObtenerTiposSuscripciones")]
        public async Task<IActionResult> ObtenerTiposSuscripciones()
        {
            try
            {
                var resultados = from ts in _baseDatos.TipoSuscripcion
                                 select new
                                 {
                                     Id = ts.Id,
                                     Nombre = ts.Nombre,
                                     Descripcion = ts.Descripcion,
                                     Precio = ts.Precio,
                                     Duracion = ts.Duracion,
                                     Estatus = ts.Estatus == "1" ? "Activo" : "Inactivo"
                                 };


                return Ok(resultados);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { CodigoHttp = 500, mensaje = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("ObtenerMedicosRegistrados")]
        public async Task<IActionResult> ObtenerMedicos()
        {
            try
            {
                var listaMedicos = await _baseDatos.Medicos
                    .Include(m => m.EspecialidadMedicoIntermedia)
                    .ThenInclude(em => em.IdEspecialidadNavigation)
                    .Where(m => m.Estatus == "1") // Asegúrate de que Estatus es un string
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
                    FechaRegistro = m.FechaRegistro?.ToString("yyyy-MM-dd"),
                    Especialidades = m.EspecialidadMedicoIntermedia.Select(em => new
                    {
                        em.IdEspecialidad,
                        em.NumCedula,
                        em.Honorarios,
                        Especialidad = em.IdEspecialidadNavigation?.Nombre
                    })
                });

                return Ok(new
                {
                    mensaje = "Médicos obtenidos exitosamente.",
                    estatus = "success",
                    data = listaMedicosDTO
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor.",
                    estatus = "error",
                    error = ex.Message
                });
            }
        }

        // Actualizar Solicitud Medico -----------------------------------------------------------------------------------------------
        [HttpPut]
        [Route("ActualizarSolicitudMedico/{id}")]
        public async Task<IActionResult> ActualizarSolicitudMedico(int id)
        {
            try
            {
                var medico = await _baseDatos.Medicos.FirstOrDefaultAsync(m => m.Id == id);

                if (medico == null)
                {
                    return NotFound(new
                    {
                        mensaje = $"No se encontró un médico con el ID {id}.",
                        estatus = "error",
                        data = (object)null
                    });
                }

                medico.Estatus = "3";

                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Médico actualizado correctamente.",
                    estatus = "success",
                    data = new
                    {
                        medico.Id,
                        medico.Nombre,
                        medico.Apellido,
                        medico.Estatus
                        // Agrega otros campos si es necesario
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor.",
                    estatus = "error",
                    data = ex.Message
                });
            }
        }

    }
}
