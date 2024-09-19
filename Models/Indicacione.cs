using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Indicacione
{
    public int Id { get; set; }

    public int? IdTratamiento { get; set; }

    public int? Dia { get; set; }

    public TimeOnly? Hora { get; set; }

    public string? Descripcion { get; set; }

    public virtual Tratamiento? IdTratamientoNavigation { get; set; }
}
