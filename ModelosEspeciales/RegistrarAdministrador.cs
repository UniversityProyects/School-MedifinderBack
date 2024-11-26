using System.ComponentModel.DataAnnotations;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarAdministrador
    {
        public class AdministradorDTO
        {
            public string? Nombre { get; set; }

            public string? Apellido { get; set; }

            public string? Email { get; set; }

            public string? Contrasena { get; set; }
        }

        public class ModificarAdministradorDTO
        {
            public string? Nombre { get; set; }

            public string? Apellido { get; set; }

            public string? Email { get; set; }
        }

        public class ModificarContrasenaDTO
        {
            public string? ContrasenaActual { get; set; }
            public string? NuevaContrasena { get; set; }
        }

        public class LoginAdmonDTO
        {
            [Required(ErrorMessage = "El campo Email es requerido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "El campo Contraseña es requerido")]
            public string Contrasena { get; set; }
        }
    }
}
