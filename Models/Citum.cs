using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Citum
{
    public int Id { get; set; }

    public int? IdPaciente { get; set; }

    public int? IdMedico { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Descripcion { get; set; }

    public string? Estatus { get; set; }

    public DateTime? FechaCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }

    public virtual ICollection<CalificacionMedico> CalificacionMedicos { get; set; } = new List<CalificacionMedico>();

    public virtual ICollection<HistorialClinico> HistorialClinicos { get; set; } = new List<HistorialClinico>();

    public virtual Medico? IdMedicoNavigation { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }
}
