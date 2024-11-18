using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Historial
{
    public int IdHistorial { get; set; }

    public int? IdMedico { get; set; }

    public int? IdAdministrador { get; set; }

    public DateOnly? Fecha { get; set; }

    public string? Observaciones { get; set; }

    public int? IdMedioContacto { get; set; }

    public int? NivelSatisfaccion { get; set; }

    public virtual Administrador? IdAdministradorNavigation { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }

    public virtual MedioContacto? IdMedioContactoNavigation { get; set; }
}
