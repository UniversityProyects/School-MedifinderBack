using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Paciente
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Email { get; set; }

    public string? Contrasena { get; set; }

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public string? Sexo { get; set; }

    public string? Estatus { get; set; }

    public virtual ICollection<Citum> Cita { get; set; } = new List<Citum>();

    public virtual ICollection<PacientesAsignado> PacientesAsignados { get; set; } = new List<PacientesAsignado>();
}
