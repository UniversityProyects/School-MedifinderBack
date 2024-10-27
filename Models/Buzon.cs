using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Buzon
{
    public int IdSoliditudBuzon { get; set; }

    public int IdMedico { get; set; }

    public int IdPaciente { get; set; }

    public string Comentario { get; set; } = null!;

    public DateOnly FechaRegistro { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public int? Estatus { get; set; }

    public virtual ICollection<BuzonClasificacionComentario> BuzonClasificacionComentarios { get; set; } = new List<BuzonClasificacionComentario>();

    public virtual ICollection<BuzonTipoComentario> BuzonTipoComentarios { get; set; } = new List<BuzonTipoComentario>();
}
