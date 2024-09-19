using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class EspecialidadMedicoIntermedium
{
    public int Id { get; set; }

    public int? IdEspecialidad { get; set; }

    public int? IdMedico { get; set; }

    public string? NumCedula { get; set; }

    public decimal? Honorarios { get; set; }

    public virtual Especialidad? IdEspecialidadNavigation { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }
}
