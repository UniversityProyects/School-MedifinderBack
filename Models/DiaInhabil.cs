using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class DiaInhabil
{
    public int Id { get; set; }

    public int? IdMedico { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }
}
