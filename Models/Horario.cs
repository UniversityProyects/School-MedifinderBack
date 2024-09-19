using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Horario
{
    public int Id { get; set; }

    public int? IdMedico { get; set; }

    public int? Dia { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }
}
