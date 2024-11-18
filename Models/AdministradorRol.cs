using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class AdministradorRol
{
    public int Id { get; set; }

    public int? IdAdministrador { get; set; }

    public int? EsAdminMaestro { get; set; }

    public virtual Administrador? IdAdministradorNavigation { get; set; }
}
