using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class TipoComentario
{
    public int IdTipoComentario { get; set; }

    public string Tipo { get; set; } = null!;

    public DateOnly FechaRegistro { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public int? Estatus { get; set; }

    public virtual ICollection<BuzonTipoComentario> BuzonTipoComentarios { get; set; } = new List<BuzonTipoComentario>();
}
