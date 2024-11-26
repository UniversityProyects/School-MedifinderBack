using System.ComponentModel.DataAnnotations;

namespace MediFinder_Backend.ModelosEspeciales
{
    public class CRMWeb
    {

        public class HistorialDTO
        {
            [Required]
            public int IdMedico { get; set; }

            [Required]
            public int IdAdministrador { get; set; }

            [Required]
            [StringLength(500, ErrorMessage = "Las observaciones no pueden tener más de 500 caracteres.")]
            public string Observaciones { get; set; }

            [Required]
            public int IdMedioContacto { get; set; }

            [Range(1, 5, ErrorMessage = "El nivel de satisfacción debe estar entre 1 y 5.")]
            public int NivelSatisfaccion { get; set; }
        }


    }
}
