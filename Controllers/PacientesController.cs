using MediFinder_Backend.ModelosEspeciales;
using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarPacientes;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : Controller
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public PacientesController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        // Registrar paciente --------------------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarPaciente([FromBody] PacienteDTO pacienteDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var paciente = new Paciente
                {
                    Nombre = pacienteDTO.Nombre,
                    Apellido = pacienteDTO.Apellido,
                    Email = pacienteDTO.Email,
                    Contrasena = pacienteDTO.Contrasena,
                    Telefono = pacienteDTO.Telefono,
                    FechaNacimiento = pacienteDTO.FechaNacimiento,
                    Sexo = pacienteDTO.Sexo,
                    Estatus = pacienteDTO.Estatus,
                };

                // Guardar el médico en la base de datos
                _baseDatos.Paciente.Add(paciente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Paciente registrado correctamente", paciente.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        // Login de pacientes registrados ------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginPDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var paciente = await _baseDatos.Paciente
                    .FirstOrDefaultAsync(p => p.Email == loginDTO.Email && p.Contrasena == loginDTO.Contrasena);

                if (paciente == null)
                {
                    return NotFound("Correo electrónico o contraseña incorrectos.");
                }

                var pacienteDTO = new
                {   
                    id   = paciente.Id,
                    NombreCompleto = $"{paciente.Nombre} {paciente.Apellido}",
                    Email = paciente.Email,
                    Telefono = paciente.Telefono,
                    FechaNacimiento = paciente.FechaNacimiento?.ToString("yyyy-MM-dd"),
                    Sexo = paciente.Sexo,
                    Estatus = paciente.Estatus
                };

                return Ok(pacienteDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("LoginWear")]
        public async Task<IActionResult> LoginWear([FromBody] LoginPDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var paciente = await _baseDatos.Paciente
                    .FirstOrDefaultAsync(p => p.Email == loginDTO.Email && p.Contrasena == loginDTO.Contrasena);

                if (paciente == null)
                {
                    return NotFound("Correo electrónico o contraseña incorrectos.");
                }

                var pacienteDTO = new
                {
                    id = paciente.Id,
                    NombreCompleto = $"{paciente.Nombre} {paciente.Apellido}",
                    Email = paciente.Email,
                    Telefono = paciente.Telefono,
                    FechaNacimiento = paciente.FechaNacimiento?.ToString("yyyy-MM-dd"),
                    Sexo = paciente.Sexo,
                    Estatus = paciente.Estatus,
                    Contrasena = loginDTO.Contrasena
                };

                return Ok(pacienteDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("ModificarPaciente/{id}")]
        public async Task<IActionResult> ModificarPaciente(int id, [FromBody] PacienteDTO pacienteDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Buscar al paciente en la base de datos por su ID
                var paciente = await _baseDatos.Paciente.FindAsync(id);

                if (paciente == null)
                {
                    return NotFound($"No se encontró ningún paciente con el ID {id}");
                }

                // Actualizar los datos del paciente con los nuevos valores
                paciente.Nombre = pacienteDTO.Nombre;
                paciente.Apellido = pacienteDTO.Apellido;
                paciente.Email = pacienteDTO.Email;
                paciente.Contrasena = pacienteDTO.Contrasena;
                paciente.Telefono = pacienteDTO.Telefono;
                paciente.FechaNacimiento = pacienteDTO.FechaNacimiento;
                paciente.Sexo = pacienteDTO.Sexo;
                paciente.Estatus = pacienteDTO.Estatus;

                // Guardar los cambios en la base de datos
                _baseDatos.Paciente.Update(paciente);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = $"Paciente con ID {id} modificado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        // Obtener lista de pacientes registrados ------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("ObtenerPacientes")]
        public async Task<IActionResult> ObtenerPacientes()
        {
            try
            {
                var listaPacientes = await _baseDatos.Paciente
                    .ToListAsync();

                return Ok(listaPacientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Consultar paciente por ID -------------------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("ObtenerPacientePorId/{id}")]
        public async Task<IActionResult> ObtenerPacientePorId(int id)
        {
            try
            {
                var paciente = await _baseDatos.Paciente
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (paciente == null)
                {
                    return NotFound($"No se encontró el paciente con ID {id}");
                }

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }



        // Método para pacientes ***********************************************************************************************************
        private bool PacienteExistsId(int id)
        {
            return _baseDatos.Paciente.Any(p => p.Id == id);
        }

        private async Task<Boolean> ExistePaciente(string email, string nombre, string apellido)
        {
            var pacienteExistente = await _baseDatos.Paciente
                .AnyAsync(p => p.Email == email || (p.Nombre == nombre && p.Apellido == apellido));

            if (pacienteExistente != null)
            {
                return true;
            }
            return false;
        }

    }
}
