namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarTiposSuscripciones
    {
        public class TipoSuscripcionDTO
        {
            public int Id { get; set; }

            public string? Nombre { get; set; }

            public string? Descripcion { get; set; }

            public decimal? Precio { get; set; }

            public int? Duracion { get; set; }
        }
    }
}
