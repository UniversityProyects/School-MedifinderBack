using System.ComponentModel.DataAnnotations;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class BuzonRequest
    {
        public class BuzonRequestDto
        {
            public int? IdMedico { get; set; }
            public int? IdPaciente { get; set; }
            public string Comentario { get; set; } = string.Empty;
            public List<int> TipoComentariosIds { get; set; } = new List<int>();
        }

    }
    public class SolicitudBuzonResponseDto
    {
        public int IdSolicitudBuzon { get; set; }
        public int? IdMedico { get; set; }
        public int? IdPaciente { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int Estatus { get; set; }
        public List<TipoComentarioDto> TiposComentarios { get; set; } = new List<TipoComentarioDto>();
        public List<ClasificacionComentarioDto> ClasificacionesComentarios { get; set; } = new List<ClasificacionComentarioDto>();
    }

    public class TipoComentarioDto
    {
        public int IdTipoComentario { get; set; }
        public string Nombre { get; set; }
    }

    public class ClasificacionComentarioDto
    {
        public int IdClasificacionComentario { get; set; }
        public string Nombre { get; set; }
    }
}
