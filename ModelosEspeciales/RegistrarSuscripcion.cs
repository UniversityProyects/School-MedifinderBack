namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarSuscripcion
    {
        public class SuscripcionDTO
        {
            public int Id { get; set; }

            public int? IdTipoSuscripcion { get; set; }

            public int? IdMedico { get; set; }

            public DateOnly? FechaInicio { get; set; }

        }

    }
}
