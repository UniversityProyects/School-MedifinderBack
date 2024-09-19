using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Tratamiento
{
    public int Id { get; set; }

    public int? IdHistorialClinico { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Descripcion { get; set; }

    public string? Estatus { get; set; }

    public virtual HistorialClinico? IdHistorialClinicoNavigation { get; set; }

    public virtual ICollection<Indicacione> Indicaciones { get; set; } = new List<Indicacione>();
}
