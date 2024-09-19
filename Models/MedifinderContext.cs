using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MediFinder_Backend.Models;

public partial class MedifinderContext : DbContext
{
    public MedifinderContext()
    {
    }

    public MedifinderContext(DbContextOptions<MedifinderContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrador> Administrador { get; set; }

    public virtual DbSet<CalificacionMedico> CalificacionMedicos { get; set; }

    public virtual DbSet<Citum> Cita { get; set; }

    public virtual DbSet<DiaInhabil> DiaInhabil { get; set; }

    public virtual DbSet<Especialidad> Especialidad { get; set; }

    public virtual DbSet<EspecialidadMedicoIntermedium> EspecialidadMedicoIntermedia { get; set; }

    public virtual DbSet<HistorialClinico> HistorialClinico { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Indicacione> Indicaciones { get; set; }

    public virtual DbSet<Medico> Medicos { get; set; }

    public virtual DbSet<Paciente> Paciente { get; set; }

    public virtual DbSet<PacientesAsignado> PacientesAsignados { get; set; }

    public virtual DbSet<PagoSuscripcion> PagoSuscripcion { get; set; }

    public virtual DbSet<Suscripcion> Suscripcion { get; set; }

    public virtual DbSet<TipoSuscripcion> TipoSuscripcion { get; set; }

    public virtual DbSet<Tratamiento> Tratamiento { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Administ__3214EC071D62D458");

            entity.ToTable("Administrador");

            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CalificacionMedico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Califica__3214EC07EBD67165");

            entity.ToTable("Calificacion_Medico");

            entity.Property(e => e.Comentarios).IsUnicode(false);
            entity.Property(e => e.IdCita).HasColumnName("Id_Cita");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.CalificacionMedicos)
                .HasForeignKey(d => d.IdCita)
                .HasConstraintName("FK_Calificacion_Cita");
        });

        modelBuilder.Entity<Citum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cita__3214EC076CA25374");

            entity.Property(e => e.Descripcion).IsUnicode(false);
            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaCancelacion)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Cancelacion");
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Inicio");
            entity.Property(e => e.IdMedico).HasColumnName("Id_Medico");
            entity.Property(e => e.IdPaciente).HasColumnName("Id_Paciente");
            entity.Property(e => e.MotivoCancelacion)
                .IsUnicode(false)
                .HasColumnName("Motivo_Cancelacion");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("FK_Cita_Medico");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdPaciente)
                .HasConstraintName("FK_Cita_Paciente");
        });

        modelBuilder.Entity<DiaInhabil>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Dia_Inha__3214EC0701B137D2");

            entity.ToTable("Dia_Inhabil");

            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.IdMedico).HasColumnName("Id_Medico");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.DiaInhabils)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("FK_Inhabil_Medico");
        });

        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Especial__3214EC0701A71603");

            entity.ToTable("Especialidad");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EspecialidadMedicoIntermedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Especial__3214EC07274FBAEC");

            entity.ToTable("Especialidad_Medico_Intermedia");

            entity.Property(e => e.Honorarios).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdEspecialidad).HasColumnName("Id_Especialidad");
            entity.Property(e => e.IdMedico).HasColumnName("Id_Medico");
            entity.Property(e => e.NumCedula)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Num_Cedula");

            entity.HasOne(d => d.IdEspecialidadNavigation).WithMany(p => p.EspecialidadMedicoIntermedia)
                .HasForeignKey(d => d.IdEspecialidad)
                .HasConstraintName("FK_Especialidad_Intermedia");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.EspecialidadMedicoIntermedia)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("FK_Medico_Intermedia");
        });

        modelBuilder.Entity<HistorialClinico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Historia__3214EC0732D59257");

            entity.ToTable("Historial_Clinico");

            entity.Property(e => e.Diagnostico).IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.GlucosaPaciente)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Glucosa_Paciente");
            entity.Property(e => e.IdCita).HasColumnName("Id_Cita");
            entity.Property(e => e.Intervenciones).IsUnicode(false);
            entity.Property(e => e.Observaciones).IsUnicode(false);
            entity.Property(e => e.OxigenacionPaciente)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Oxigenacion_Paciente");
            entity.Property(e => e.Padecimientos).IsUnicode(false);
            entity.Property(e => e.PesoPaciente)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Peso_Paciente");
            entity.Property(e => e.PresionPaciente)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Presion_Paciente");
            entity.Property(e => e.TallaPaciente)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Talla_Paciente");
            entity.Property(e => e.TemperaturaCorporalPaciente)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Temperatura_Corporal_Paciente");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.HistorialClinicos)
                .HasForeignKey(d => d.IdCita)
                .HasConstraintName("FK_Historial_Cita");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Horario__3214EC0768F2A01E");

            entity.ToTable("Horario");

            entity.Property(e => e.HoraFin).HasColumnName("Hora_Fin");
            entity.Property(e => e.HoraInicio).HasColumnName("Hora_Inicio");
            entity.Property(e => e.IdMedico).HasColumnName("Id_Medico");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.Horarios)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("FK_Horario_Medico");
        });

        modelBuilder.Entity<Indicacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Indicaci__3214EC0749F59623");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.IdTratamiento).HasColumnName("Id_Tratamiento");

            entity.HasOne(d => d.IdTratamientoNavigation).WithMany(p => p.Indicaciones)
                .HasForeignKey(d => d.IdTratamiento)
                .HasConstraintName("FK_Indicaciones_Tratamiento");
        });

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medico__3214EC07A6A55E86");

            entity.ToTable("Medico");

            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Calle)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Codigo_Postal");
            entity.Property(e => e.Colonia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaBaja)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Baja");
            entity.Property(e => e.FechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Registro");
            entity.Property(e => e.FechaValidacion)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Validacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Numero)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paciente__3214EC0717BF5232");

            entity.ToTable("Paciente");

            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaNacimiento).HasColumnName("Fecha_Nacimiento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Sexo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PacientesAsignado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paciente__3214EC07824786D7");

            entity.ToTable("Pacientes_Asignados");

            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaFinalizacion)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Finalizacion");
            entity.Property(e => e.IdMedico).HasColumnName("Id_Medico");
            entity.Property(e => e.IdPaciente).HasColumnName("Id_Paciente");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.PacientesAsignados)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("FK_PacienteAsignado_Medico");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.PacientesAsignados)
                .HasForeignKey(d => d.IdPaciente)
                .HasConstraintName("FK_PacienteAsignado_Paciente");
        });

        modelBuilder.Entity<PagoSuscripcion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pago_Sus__3214EC07A0F5989A");

            entity.ToTable("Pago_Suscripcion");

            entity.Property(e => e.FechaPago).HasColumnName("Fecha_Pago");
            entity.Property(e => e.IdSuscripcion).HasColumnName("Id_Suscripcion");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdSuscripcionNavigation).WithMany(p => p.PagoSuscripcions)
                .HasForeignKey(d => d.IdSuscripcion)
                .HasConstraintName("FK_Pago_Suscripcion");
        });

        modelBuilder.Entity<Suscripcion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Suscripc__3214EC07D8B3D9BC");

            entity.ToTable("Suscripcion");

            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin).HasColumnName("Fecha_Fin");
            entity.Property(e => e.FechaInicio).HasColumnName("Fecha_Inicio");
            entity.Property(e => e.IdMedico).HasColumnName("Id_Medico");
            entity.Property(e => e.IdTipoSuscripcion).HasColumnName("Id_Tipo_Suscripcion");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.Suscripcions)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("FK_Suscripcion_Medico");

            entity.HasOne(d => d.IdTipoSuscripcionNavigation).WithMany(p => p.Suscripcions)
                .HasForeignKey(d => d.IdTipoSuscripcion)
                .HasConstraintName("KF_Suscripcion_Tipo");
        });

        modelBuilder.Entity<TipoSuscripcion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tipo_Sus__3214EC0752DEDE81");

            entity.ToTable("Tipo_Suscripcion");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Tratamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tratamie__3214EC075002E146");

            entity.ToTable("Tratamiento");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Estatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Fin");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Inicio");
            entity.Property(e => e.IdHistorialClinico).HasColumnName("Id_Historial_Clinico");

            entity.HasOne(d => d.IdHistorialClinicoNavigation).WithMany(p => p.Tratamientos)
                .HasForeignKey(d => d.IdHistorialClinico)
                .HasConstraintName("FK_Tratamiento_Historial");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
