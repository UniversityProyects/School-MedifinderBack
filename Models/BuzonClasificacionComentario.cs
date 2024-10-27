using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class BuzonClasificacionComentario
{
    public int IdBuzonClasificacionComentario { get; set; }

    public int IdSoliditudBuzon { get; set; }

    public int IdClasificacionComentario { get; set; }

    public virtual ClasificacionComentario IdClasificacionComentarioNavigation { get; set; } = null!;

    public virtual Buzon IdSoliditudBuzonNavigation { get; set; } = null!;
}
