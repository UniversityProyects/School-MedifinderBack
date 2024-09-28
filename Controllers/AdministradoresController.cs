using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarAdministrador;
using static MediFinder_Backend.ModelosEspeciales.RegistrarMedico;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministradoresController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        //Contructor del controlador
        public AdministradoresController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Registrar Administrador ----------------------------------------------
        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarAdministrador([FromBody] AdministradorDTO administradorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Permite validar si ya existe una cuenta registrada.
                if (await ExisteAdministrador(administradorDTO.Nombre, administradorDTO.Apellido, administradorDTO.Email))
                {
                    return BadRequest($"Ya existe un administrador con el mismo nombre o correo electrónico.");
                }

                //Formateamod los valores
                var administradorNuevo = new Administrador
                {
                    Nombre = administradorDTO.Nombre,
                    Apellido = administradorDTO.Apellido,
                    Email = administradorDTO.Email,
                    Contrasena = administradorDTO.Contrasena,
                    Estatus = "1"
                };

                // Guardar el médico en la base de datos
                _baseDatos.Administrador.Add(administradorNuevo);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Administrador registrado correctamente", administradorNuevo.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Verificar Login Administrador -----------------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginAdmonDTO loginAdmonDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Busca el administrador por correo electrónico y contraseña
                var administrador = await _baseDatos.Administrador
                    .FirstOrDefaultAsync(a => a.Email == loginAdmonDTO.Email && a.Contrasena == loginAdmonDTO.Contrasena);

                if (administrador == null)
                {
                    return Unauthorized("Credenciales incorrectas. Por favor, verifique su correo electrónico y contraseña.");
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
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Modificar informacion administrador ----------------------------------------
        [HttpPut]
        [Route("Modificar/{id}")]
        public async Task<IActionResult> ModificarAdministrador(int id, [FromBody] ModificarAdministradorDTO modificarAdministradorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existeAdministrador = await _baseDatos.Administrador
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (existeAdministrador == null)
                {
                    return NotFound($"No existe ningún administrador con el Id {id}");
                }

                var verificarAdmin = await _baseDatos.Administrador
                .FirstOrDefaultAsync(m => 
                (m.Nombre.ToLower().Trim() == modificarAdministradorDTO.Nombre.ToLower().Trim() 
                    && m.Apellido.ToLower().Trim() == modificarAdministradorDTO.Apellido.ToLower().Trim()) 
                    || m.Email.ToLower().Trim() == modificarAdministradorDTO.Email.ToLower().Trim()
                    && m.Id != id);

                //Permite validar si ya existe una cuenta registrada.
                if (verificarAdmin != null)
                {
                    return BadRequest($"Ya existe un administrador con el mismo nombre o correo electrónico.");
                }

                //Seteamos la informacion nueva
                existeAdministrador.Nombre = modificarAdministradorDTO.Nombre;
                existeAdministrador.Apellido = modificarAdministradorDTO.Apellido;
                existeAdministrador.Email = modificarAdministradorDTO.Email;

                //Guardamos la informacion nueva
                await _baseDatos.SaveChangesAsync();

                //retornamos mensaje de confirmacion
                return Ok(new { message = $"El administrador con el id {existeAdministrador.Id} ha sido modificado correctamente." });


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Modificar contraseña administrador -----------------------------------------
        [HttpPut]
        [Route("ModificarContrasena/{id}")]
        public async Task<IActionResult> ModificarContrasena(int id, [FromBody] ModificarContrasenaDTO modificarContrasenaDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Buscamos al administrador en la BD
                var existeAdministrador = await _baseDatos.Administrador
                    .FirstOrDefaultAsync(a => a.Id == id);

                //Verificamos si existe
                if (existeAdministrador == null)
                {
                    return NotFound($"No existe ningún administrador con el Id {id}");
                }

                //Verificamos si la contraseña actual ingresada es correcta
                if (existeAdministrador.Contrasena != modificarContrasenaDTO.ContrasenaActual)
                {
                    return Unauthorized($"La contraseña actual ingresada es incorrecta.");
                }

                //Seteamos la nueva contraseña
                existeAdministrador.Contrasena = modificarContrasenaDTO.NuevaContrasena;

                //Guardamos la informacion nueva
                await _baseDatos.SaveChangesAsync();

                //retornamos mensaje de confirmacion
                return Ok(new { message = $"Contraseña actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Listado de administradores (Activos) ---------------------------------------
        [HttpGet]
        [Route("ListadoAdministradores")]
        public async Task<IActionResult> ObtenerAdministradores()
        {
            try
            {
                var listadoAdministradores = await _baseDatos.Administrador
                    .Where(a => a.Estatus == "1").ToListAsync();

                if (listadoAdministradores.Count == 0)
                {
                    return NotFound(new { message = "No hay ningún administrador registrado" });
                }

                return Ok(listadoAdministradores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
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
    }
}
