using System.ComponentModel.DataAnnotations;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarMedico
    {

        public class MedicoDTO
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string Contrasena { get; set; }
            public string Telefono { get; set; }
            public string Calle { get; set; }
            public string Colonia { get; set; }
            public string Numero { get; set; }
            public string Ciudad { get; set; }
            public string Pais { get; set; }
            public string Codigo_Postal { get; set; }
            public string Avatar { get; set; }

            public List<EspecialidadDTO> Especialidades { get; set; }
        }


        public class EspecialidadDTO
        {
            public int Id_Especialidad { get; set; }
            public string Num_Cedula { get; set; }
            public decimal Honorarios { get; set; }
        }
        /*
        public class RegistrarHorasDTO
        {
            public int IdMedico { get; set; }  
            public int DiaSemana { get; set; }
            public string HoraInicio { get; set; }
            public string HoraFin { get; set; }  
        }
        */

        public class LoginDTO
        {
            [Required(ErrorMessage = "El campo Email es requerido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "El campo Contraseña es requerido")]
            public string Contrasena { get; set; }
        }

        public class RegistrarHorasDTO
        {
            public int IdMedico { get; set; }
            public List<DiaSeleccionadoDTO> DiasSeleccionados { get; set; }
            public string HoraInicio { get; set; }
            public string HoraFin { get; set; }
        }

        public class DiaSeleccionadoDTO
        {
            public int Id { get; set; }
            public string Dia { get; set; }
        }


    }
}
