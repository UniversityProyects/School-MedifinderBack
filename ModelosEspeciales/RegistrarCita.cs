using MediFinder_Backend.Models;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarCita
    {
        public class CitaDTO
        {

            public int? IdPaciente { get; set; }

            public int? IdMedico { get; set; }

            public DateTime? FechaInicio { get; set; }

            public DateTime? FechaFin { get; set; }

            public string? Descripcion { get; set; }

            public DateTime? FechaCancelacion { get; set; }

            public string? MotivoCancelacion { get; set; }
        }

        public class CitaInfoDto
        {
            public int Id { get; set; }

            public int? IdPaciente { get; set; }

            public int? IdMedico { get; set; }

            public DateTime? FechaInicio { get; set; }

            public DateTime? FechaFin { get; set; }

            public string? Descripcion { get; set; }
            public string Estatus {  get; set; }

            public DateTime? FechaCancelacion { get; set; }

            public string? MotivoCancelacion { get; set; }
            public string NombrePaciente { get; set; }
            public string ApellidoPaciente { get; set; }
            public string NombreMedico { get; set; }
            public string ApellidoMedico { get; set; }
        }

        public class CancelarCitaDTO
        {
            public string MotivoCancelacion { get; set; }
        }
    }
}
