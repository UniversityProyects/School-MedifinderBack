using MediFinder_Backend.Models;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarCalificacionMedicos
    {
        public class CalificacionMedicoDTO
        {

            public int? IdCita { get; set; }

            public int? Puntuacion { get; set; }

            public string? Comentarios { get; set; }

        }
    }
}
