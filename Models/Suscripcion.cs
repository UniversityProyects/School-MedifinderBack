using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Suscripcion
{
    public int Id { get; set; }

    public int? IdTipoSuscripcion { get; set; }

    public int? IdMedico { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Estatus { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }

    public virtual TipoSuscripcion? IdTipoSuscripcionNavigation { get; set; }

    public virtual ICollection<PagoSuscripcion> PagoSuscripcions { get; set; } = new List<PagoSuscripcion>();
}
