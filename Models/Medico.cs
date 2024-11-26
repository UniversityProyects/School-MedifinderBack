using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Medico
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Email { get; set; }

    public string? Contrasena { get; set; }

    public string? Telefono { get; set; }

    public string? Calle { get; set; }

    public string? Colonia { get; set; }

    public string? Numero { get; set; }

    public string? Ciudad { get; set; }

    public string? Pais { get; set; }

    public string? CodigoPostal { get; set; }

    public string? Estatus { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? FechaValidacion { get; set; }

    public DateTime? FechaBaja { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<AdministradorMedicoAutorizado> AdministradorMedicoAutorizados { get; set; } = new List<AdministradorMedicoAutorizado>();

    public virtual ICollection<Citum> Cita { get; set; } = new List<Citum>();

    public virtual ICollection<DiaInhabil> DiaInhabils { get; set; } = new List<DiaInhabil>();

    public virtual ICollection<EspecialidadMedicoIntermedium> EspecialidadMedicoIntermedia { get; set; } = new List<EspecialidadMedicoIntermedium>();

    public virtual ICollection<Historial> Historials { get; set; } = new List<Historial>();

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public virtual ICollection<PacientesAsignado> PacientesAsignados { get; set; } = new List<PacientesAsignado>();

    public virtual ICollection<Suscripcion> Suscripcions { get; set; } = new List<Suscripcion>();
}
