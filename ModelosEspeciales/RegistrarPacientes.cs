using System.ComponentModel.DataAnnotations;
using static MediFinder_Backend.ModelosEspeciales.RegistrarMedico;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarPacientes
    {

        public class PacienteDTO
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string? Contrasena { get; set; }
            public string Telefono { get; set; }
            public DateOnly FechaNacimiento { get; set; } 
            public string Sexo { get; set; }
            public string? Estatus { get; set; }
        }
        public class LoginPDTO
        {
            [Required(ErrorMessage = "El campo Email es requerido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "El campo Contraseña es requerido")]
            public string Contrasena { get; set; }
        }
    }
}
