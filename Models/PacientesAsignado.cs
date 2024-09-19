using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class PacientesAsignado
{
    public int Id { get; set; }

    public int? IdMedico { get; set; }

    public int? IdPaciente { get; set; }

    public DateOnly? Fecha { get; set; }

    public string? Estatus { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }
}
