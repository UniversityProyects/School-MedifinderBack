using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class PagoSuscripcion
{
    public int Id { get; set; }

    public int? IdSuscripcion { get; set; }

    public decimal? Monto { get; set; }

    public DateOnly? FechaPago { get; set; }

    public virtual Suscripcion? IdSuscripcionNavigation { get; set; }
}
