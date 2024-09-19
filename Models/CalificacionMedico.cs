using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class CalificacionMedico
{
    public int Id { get; set; }

    public int? IdCita { get; set; }

    public int? Puntuacion { get; set; }

    public DateOnly? Fecha { get; set; }

    public string? Comentarios { get; set; }

    public virtual Citum? IdCitaNavigation { get; set; }
}
