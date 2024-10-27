using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class ClasificacionComentario
{
    public int IdClasificacionComentario { get; set; }

    public string Clasificacion { get; set; } = null!;

    public DateOnly? FechaRegistro { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public int? Estatus { get; set; }

    public virtual ICollection<BuzonClasificacionComentario> BuzonClasificacionComentarios { get; set; } = new List<BuzonClasificacionComentario>();
}
