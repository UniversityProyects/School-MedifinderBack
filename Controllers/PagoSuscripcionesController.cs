using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.RegistrarPagoSuscripcion;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagoSuscripcionesController : ControllerBase
    {
        //Variable de ccontexto de BD
        private readonly MedifinderContext _baseDatos;

        public PagoSuscripcionesController(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //Pagar suscripciones --------------------------------------------------------
        [HttpPost]
        [Route("RegistrarPago")]
        public async Task<IActionResult> RegistrarPagoSuscripcion([FromBody] PagoSuscripcionDTO pagoSuscripcionDTO)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Verificar si existe la suscripcion que se va a pagar
                var existeSuscripcion = await (
                     from s in _baseDatos.Suscripcion
                     join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                     where s.Id == pagoSuscripcionDTO.IdSuscripcion
                     select new
                     {
                         Precio = ts.Precio
                     }
                 ).FirstOrDefaultAsync();

                if (existeSuscripcion == null)
                {
                    return NotFound($"La suscripcion recibida no existe");
                }

                //Verificar el monto que se esta pagando
                if (existeSuscripcion.Precio != pagoSuscripcionDTO.Monto)
                {
                    return BadRequest($"El monto recibido no coincide con el precio de la suscripción.");
                }

                //Formateamos el nuevo registros
                var pagoSuscripcionNuevo = new PagoSuscripcion
                {
                    IdSuscripcion = pagoSuscripcionDTO.IdSuscripcion,
                    Monto = pagoSuscripcionDTO.Monto,
                    FechaPago = DateOnly.FromDateTime(DateTime.Now)
                };

                //Guardamos en la Bd
                _baseDatos.PagoSuscripcion.Add(pagoSuscripcionNuevo);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "El pago de la suscripción ha sido registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Listado de pagos por tipo de suscripcion -----------------------------------
        [HttpGet]
        [Route("ObtenerPagosPorTipoSuscripcion/{idTipoSuscripcion}")]
        public async Task<IActionResult> ObtenerPagosPorTipoSuscripcion(int idTipoSuscripcion)
        {
            try
            {
                var resultado = from ps in _baseDatos.PagoSuscripcion
                                join s in _baseDatos.Suscripcion on ps.IdSuscripcion equals s.Id
                                join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                                join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                                where s.IdTipoSuscripcion == 1
                                select new
                                {
                                    ps.Id,
                                    ps.Monto,
                                    ps.FechaPago,
                                    ps.IdSuscripcion,
                                    s.FechaInicio,
                                    s.FechaFin,
                                    s.Estatus,
                                    s.IdTipoSuscripcion,
                                    ts.Nombre,
                                    ts.Descripcion,
                                    ts.Duracion,
                                    ts.Precio,
                                    s.IdMedico,
                                    NombreMedico = m.Nombre,
                                    ApellidoMedico = m.Apellido
                                };

                var listaResultados = await resultado.ToListAsync();

                // Validamos si la lista contiene algo
                if (listaResultados.Count == 0)
                {
                    return NotFound($"No se encontraron registros de pagos para el tipo de suscripción.");
                }

                // Agrupamos los resultados por la información del tipo de suscripción
                var agrupadosPorTipoSuscripcion = listaResultados
                    .GroupBy(r => new { r.IdTipoSuscripcion, r.Nombre, r.Descripcion, r.Duracion, r.Precio })
                    .Select(grupo => new
                    {
                        IdTipoSuscripcion = grupo.Key.IdTipoSuscripcion,
                        Nombre = grupo.Key.Nombre,
                        Descripcion = grupo.Key.Descripcion,
                        Duracion = grupo.Key.Duracion,
                        Precio = grupo.Key.Precio,
                        PagosSuscripcion = grupo.Select(r => new
                        {
                            r.Id,
                            r.Monto,
                            r.FechaPago,
                            r.IdSuscripcion,
                            r.FechaInicio,
                            r.FechaFin,
                            r.Estatus,
                            r.IdMedico,
                            r.NombreMedico,
                            r.ApellidoMedico
                        }).ToList()
                    }).ToList();

                // Retornamos los resultados agrupados
                return Ok(agrupadosPorTipoSuscripcion);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        //Listado de pagos por medico ------------------------------------------------
        [HttpGet]
        [Route("ObtenerPagosPorMedico/{idMedico}")]
        public async Task<IActionResult> ObtenerPagosPorMedico(int idMedico)
        {
            try
            {
                var resultados = await (
                    from ps in _baseDatos.PagoSuscripcion
                    join s in _baseDatos.Suscripcion on ps.IdSuscripcion equals s.Id
                    join ts in _baseDatos.TipoSuscripcion on s.IdTipoSuscripcion equals ts.Id
                    join m in _baseDatos.Medicos on s.IdMedico equals m.Id
                    where s.IdTipoSuscripcion == 1
                    select new
                    {
                        ps.Id,
                        ps.Monto,
                        ps.FechaPago,
                        ps.IdSuscripcion,
                        s.FechaInicio,
                        s.FechaFin,
                        s.Estatus,
                        s.IdTipoSuscripcion,
                        ts.Nombre,
                        ts.Descripcion,
                        ts.Duracion,
                        ts.Precio,
                        s.IdMedico,
                        NombreMedico = m.Nombre,
                        ApellidoMedico = m.Apellido
                    }
                ).ToListAsync();

                // Agrupamos los resultados por la información del médico
                var agrupadosPorMedico = resultados
                    .GroupBy(r => new { r.IdMedico, r.NombreMedico, r.ApellidoMedico })
                    .Select(grupo => new
                    {
                        IdMedico = grupo.Key.IdMedico,
                        NombreMedico = grupo.Key.NombreMedico,
                        ApellidoMedico = grupo.Key.ApellidoMedico,
                        PagosSuscripcion = grupo.Select(r => new
                        {
                            r.Id,
                            r.Monto,
                            r.FechaPago,
                            r.IdSuscripcion,
                            r.FechaInicio,
                            r.FechaFin,
                            r.Estatus,
                            r.IdTipoSuscripcion,
                            r.Nombre,
                            r.Descripcion,
                            r.Duracion,
                            r.Precio
                        }).ToList()
                    }).ToList();

                //Validamos si la lista contiene algo
                if (agrupadosPorMedico.Count == 0)
                {
                    return NotFound($"No se encontraron registros de pagos para el médico ingresado.");
                }

                // Retornamos los resultados
                return Ok(agrupadosPorMedico);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
