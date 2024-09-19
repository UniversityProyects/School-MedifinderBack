namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarHistorialClinico
    {
        public class HistorialClinicoDTO
        {
            public int Id { get; set; }

            public int? IdCita { get; set; }

            public string? Observaciones { get; set; }

            public string? Diagnostico { get; set; }

            public string? Padecimientos { get; set; }

            public string? Intervenciones { get; set; }

            public DateTime? Fecha { get; set; }

            public decimal? PesoPaciente { get; set; }

            public decimal? TallaPaciente { get; set; }

            public decimal? GlucosaPaciente { get; set; }

            public decimal? OxigenacionPaciente { get; set; }

            public decimal? PresionPaciente { get; set; }

            public decimal? TemperaturaCorporalPaciente { get; set; }
        }

        public class ChecarHistorialDTO
        {
            public int IdMedico { get; set; }
        }
    }
}
