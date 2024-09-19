using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class TipoSuscripcion
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public decimal? Precio { get; set; }

    public int? Duracion { get; set; }

    public string? Estatus { get; set; }

    public virtual ICollection<Suscripcion> Suscripcions { get; set; } = new List<Suscripcion>();
}
