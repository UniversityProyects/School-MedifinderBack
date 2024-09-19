namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarPagoSuscripcion
    {
        public class PagoSuscripcionDTO
        {
            public int Id { get; set; }

            public int? IdSuscripcion { get; set; }

            public decimal? Monto { get; set; }
        }
    }
}
