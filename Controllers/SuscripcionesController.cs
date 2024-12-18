﻿using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarCita;
using static MediFinder_Backend.ModelosEspeciales.RegistrarSuscripcion;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuscripcionesController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public SuscripcionesController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Registrar suscripcion -------------------------------------------------------
        [HttpPost]
        [Route("RegistrarSuscripcion")]
        public async Task<IActionResult> RegistrarSuscripcion([FromBody] SuscripcionDTO suscripcionDTO)
        {
            // Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verificar si el tipo de suscripción existe
                var tipoSuscripcion = await _baseDatos.TipoSuscripcion
                    .FirstOrDefaultAsync(ts => ts.Id == suscripcionDTO.IdTipoSuscripcion);

                if (tipoSuscripcion == null)
                {
                    return NotFound("El tipo de suscripción no existe.");
                }

                // Verificar si el médico existe
                var medico = await _baseDatos.Medicos
                    .FirstOrDefaultAsync(m => m.Id == suscripcionDTO.IdMedico);

                if (medico == null)
                {
                    return BadRequest("No se encontró ningún registro del médico proporcionado.");
                }

                if (medico.Estatus == "1")
                {
                    return BadRequest("No se puede registrar la suscripción porque el médico tiene pendiente su validación.");
                }

                if (medico.Estatus == "4")
                {
                    return BadRequest("No se puede registrar la suscripción porque el médico está inactivo.");
                }

                // Cambiar el estatus de todas las suscripciones previas del médico a "0"
                var suscripcionesPrevias = await _baseDatos.Suscripcion
                    .Where(s => s.IdMedico == suscripcionDTO.IdMedico && s.Estatus == "1")
                    .ToListAsync();

                foreach (var suscripcion in suscripcionesPrevias)
                {
                    suscripcion.Estatus = "0";
                }

                await _baseDatos.SaveChangesAsync();

                // Calcular fechas de inicio y fin
                var fechaInicio = DateTime.Now;
                var fechaFin = fechaInicio.AddMonths(tipoSuscripcion.Duracion ?? 0);

                // Crear y registrar nueva suscripción
                var nuevaSuscripcion = new Suscripcion
                {
                    IdTipoSuscripcion = suscripcionDTO.IdTipoSuscripcion,
                    IdMedico = suscripcionDTO.IdMedico,
                    Estatus = "1",
                    FechaInicio = DateOnly.FromDateTime(fechaInicio),
                    FechaFin = DateOnly.FromDateTime(fechaFin)
                };

                _baseDatos.Suscripcion.Add(nuevaSuscripcion);
                await _baseDatos.SaveChangesAsync();

                // Registrar pago correspondiente
                var nuevoPago = new PagoSuscripcion
                {
                    IdSuscripcion = nuevaSuscripcion.Id, // ID generado al guardar la suscripción
                    Monto = tipoSuscripcion.Precio ?? 0,
                    FechaPago = DateOnly.FromDateTime(fechaInicio)
                };

                _baseDatos.PagoSuscripcion.Add(nuevoPago);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "La suscripción y el pago se registraron exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Listado de suscripciones medico --------------------------------------------------------
        [HttpGet]
        [Route("ObtenerSucripcionesPorMedico/{idMedico}")]
        public async Task<IActionResult> ObtenerSuscripcionesPorMedico(int idMedico)
        {
            try
            {
                var existeMedico = await _baseDatos.Medicos
                    .FirstOrDefaultAsync(m => m.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound($"No se encontró registro del médico ingresado");
                }

                var resultado = from s in _baseDatos.Suscripcion
                                join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                                join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                                join ps in _baseDatos.PagoSuscripcion on s.Id equals ps.IdSuscripcion into pagos
                                from ps in pagos.DefaultIfEmpty() // Esto maneja el LEFT JOIN
                                where s.IdMedico == idMedico
                                group new { s, ts, m, ps } by new
                                {
                                    s.IdMedico,
                                    m.Nombre,
                                    m.Apellido
                                } into grupo
                                select new
                                {
                                    IdMedico = grupo.Key.IdMedico,
                                    NombreMedico = grupo.Key.Nombre,
                                    ApellidoMedico = grupo.Key.Apellido,
                                    Suscripciones = grupo.Select(g => new
                                    {
                                        g.s.Id,
                                        g.s.IdTipoSuscripcion,
                                        g.ts.Nombre,
                                        g.ts.Descripcion,
                                        g.ts.Precio,
                                        FechaInicio = g.s.FechaInicio,
                                        FechaFin = g.s.FechaFin,
                                        FechaPago = g.ps.FechaPago,
                                        Monto = g.ps.Monto,
                                        Estatus = g.s.Estatus,
                                        Pagado = g.ps.FechaPago != null ? 1 : 0
                                    }).ToList()
                                };

                var listaResultados = await resultado.ToListAsync();

                // Validamos si la lista contiene algo
                if (!listaResultados.Any())
                {
                    return NotFound($"No se encontraron registros para el médico especificado.");
                }

                // Retornamos los resultados
                return Ok(listaResultados);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Suscripcion actual medico --------------------------------------------------------------
        [HttpGet]
        [Route("ObtenerSucripcionActual/{idMedico}")]
        public async Task<IActionResult> ObtenerSuscripcionActual(int idMedico)
        {
            try
            {
                var existeMedico = await _baseDatos.Medicos
                    .FirstOrDefaultAsync(m => m.Id == idMedico);
                if (existeMedico == null)
                {
                    return NotFound($"No se encontró registro del médico ingresado");
                }

                var resultado = from s in _baseDatos.Suscripcion
                                join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                                join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                                join ps in _baseDatos.PagoSuscripcion on s.Id equals ps.IdSuscripcion into pagos
                                from ps in pagos.DefaultIfEmpty() // Maneja el LEFT JOIN
                                where s.IdMedico == idMedico && s.Estatus == "1"
                                select new
                                {
                                    s.Id,
                                    s.IdTipoSuscripcion,
                                    ts.Nombre,
                                    ts.Descripcion,
                                    ts.Precio,
                                    s.IdMedico,
                                    NombreMedico = m.Nombre,
                                    ApellidoMedico = m.Apellido,
                                    s.FechaInicio,
                                    s.FechaFin,
                                    FechaPago = ps.FechaPago,
                                    Monto = ps.Monto,
                                    Estatus = s.Estatus,
                                    Pagado = ps.FechaPago != null ? 1 : 0
                                };

                var listaResultados = await resultado.ToListAsync();

                // Validamos si la lista contiene algo
                if (!listaResultados.Any())
                {
                    return NotFound($"El médico no cuenta con una suscripción actual.");
                }

                // Retornamos los resultados
                return Ok(listaResultados);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Detalles de una suscripcion --------------------------------------------------------------
        [HttpGet]
        [Route("DetallesSuscripcion/{id}")]
        public async Task<IActionResult> DetallesSuscripcon(int id)
        {
            try
            {
                var resultado = from s in _baseDatos.Suscripcion
                                join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                                join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                                join ps in _baseDatos.PagoSuscripcion on s.Id equals ps.IdSuscripcion into pagos
                                from ps in pagos.DefaultIfEmpty() // Maneja el LEFT JOIN
                                where s.Id == id
                                select new
                                {
                                    s.Id,
                                    s.IdTipoSuscripcion,
                                    ts.Nombre,
                                    ts.Descripcion,
                                    ts.Precio,
                                    s.IdMedico,
                                    NombreMedico = m.Nombre,
                                    ApellidoMedico = m.Apellido,
                                    s.FechaInicio,
                                    s.FechaFin,
                                    FechaPago = ps.FechaPago,
                                    Monto = ps.Monto,
                                    Estatus = s.Estatus,
                                    Pagado = ps.FechaPago != null ? 1 : 0
                                };

                var listaResultados = await resultado.ToListAsync();

                // Validamos si la lista contiene algo
                if (!listaResultados.Any())
                {
                    return NotFound($"No se encontró ningún registro de la suscripción recibida.");
                }

                // Retornamos los resultados
                return Ok(listaResultados);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Cancelar suscripcion -------------------------------------------------------------------
        [HttpPut]
        [Route("CancelarSuscripcion/{id}")]
        public async Task<IActionResult> ModificarCita(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Validar que el Id de la suscripcion recibido si existe en la BD
                var existeSuscripcion = await _baseDatos.Suscripcion.FirstOrDefaultAsync(e => e.Id == id);
                if (existeSuscripcion == null)
                {
                    return NotFound($"No existe ningún registro de la suscripción recibida.");
                }

                //Actualizar el estatus de la suscripcion
                existeSuscripcion.Estatus = "0";

                //Guardamos la informacion nueva
                await _baseDatos.SaveChangesAsync();

                //retornamos mensaje de confirmacion
                return Ok(new { message = $"La suscripcion con el Id {existeSuscripcion.Id} ha sido cancelada correctamente." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        
    }
}
