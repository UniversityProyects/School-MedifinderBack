using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class MedioContacto
{
    public int IdMedioContacto { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Historial> Historials { get; set; } = new List<Historial>();
}
