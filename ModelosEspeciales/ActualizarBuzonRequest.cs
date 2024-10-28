namespace MediFinder_Backend.ModelosEspeciales
{
    public class ActualizarBuzonRequest
    {
        public int BuzonId { get; set; }
        public List<int> ClasificacionesSeleccionadas { get; set; }
    }
}
