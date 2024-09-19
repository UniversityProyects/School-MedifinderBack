using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Administrador
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Email { get; set; }

    public string? Contrasena { get; set; }

    public string? Estatus { get; set; }
}
