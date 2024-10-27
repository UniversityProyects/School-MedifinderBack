using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class BuzonTipoComentario
{
    public int IdBuzonTipoComentario { get; set; }

    public int IdSoliditudBuzon { get; set; }

    public int IdTipoComentario { get; set; }

    public virtual Buzon IdSoliditudBuzonNavigation { get; set; } = null!;

    public virtual TipoComentario IdTipoComentarioNavigation { get; set; } = null!;
}
