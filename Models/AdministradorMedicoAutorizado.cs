using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class AdministradorMedicoAutorizado
{
    public int Id { get; set; }

    public int? IdAdministrador { get; set; }

    public int? IdMedico { get; set; }

    public int? IdAutoriza { get; set; }

    public DateOnly? FechaModificacion { get; set; }

    public TimeOnly? HoraModifica { get; set; }

    public string? Estatus { get; set; }

    public virtual Administrador? IdAdministradorNavigation { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }
}
