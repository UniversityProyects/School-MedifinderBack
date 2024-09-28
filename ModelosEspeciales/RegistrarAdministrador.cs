namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarAdministrador
    {
        public class AdministradorDTO
        {
            public int Id { get; set; }

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
    }
}
