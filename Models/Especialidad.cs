using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Especialidad
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<EspecialidadMedicoIntermedium> EspecialidadMedicoIntermedia { get; set; } = new List<EspecialidadMedicoIntermedium>();
}
