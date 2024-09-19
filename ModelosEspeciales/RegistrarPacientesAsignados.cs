namespace MediFinder_Backend.ModelosEspeciales
{
    public class RegistrarPacientesAsignados
    {
        public class AsignarPacienteDTO
        {
            public int Id { get; set; }

            public int? IdMedico { get; set; }

            public int? IdPaciente { get; set; }

            public DateOnly? Fecha { get; set; }

        }
    }
}
