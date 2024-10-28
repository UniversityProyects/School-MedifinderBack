using MediFinder_Backend.ModelosEspeciales;
using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMComentariosControllert : Controller
    {
        private readonly MedifinderContext _baseDatos;

        public CRMComentariosControllert(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        [HttpGet]
        [Route("GetTiposComentarios")]
        public async Task<ActionResult<IEnumerable<TipoComentario>>> GetTiposComentarios()
        {
            var tiposComentarios = await _baseDatos.TipoComentarios.ToListAsync();
            return Ok(tiposComentarios);
        }

        [HttpGet]
        [Route("GetClasificacionesComentariosActivos")]
        public async Task<ActionResult<object>> GetClasificacionesComentariosActivos()
        {
            var clasificacionesComentarios = await _baseDatos.ClasificacionComentarios
                .Where(c => c.Estatus == 1)
                .Select(c => new
                {
                    IdClasificacionComentario = c.IdClasificacionComentario,
                    Clasificacion = c.Clasificacion
                })
                .ToListAsync();

            if (clasificacionesComentarios == null || !clasificacionesComentarios.Any())
            {
                return Ok(new
                {
                    estatus = "error",
                    mensaje = "No se encontraron clasificaciones activas.",
                    data = new List<object>()
                });
            }
            return Ok(new
            {
                estatus = "success",
                mensaje = "Clasificaciones obtenidas correctamente.",
                data = clasificacionesComentarios
            });
        }


        [HttpGet]
        [Route("GetBuzonData")]
        public async Task<IActionResult> GetBuzonData()
        {
            var resultados = await (from b in _baseDatos.Buzons
                                    join t in _baseDatos.BuzonTipoComentarios on b.IdSoliditudBuzon equals t.IdSoliditudBuzon
                                    join c in _baseDatos.TipoComentarios on t.IdTipoComentario equals c.IdTipoComentario
                                    join m in _baseDatos.Medicos on b.IdMedico equals m.Id
                                    join p in _baseDatos.Paciente on b.IdPaciente equals p.Id into pacientes
                                    from p in pacientes.DefaultIfEmpty()
                                    group new { b, c, m, p } by b.IdSoliditudBuzon into grupo
                                    select new
                                    {
                                        BuzonId = grupo.Key,
                                        TipoUsuario = grupo.First().b.IdMedico == 0 ? "Paciente" : "Medico",
                                        NombreUsuario = grupo.First().b.IdMedico == 0 ? grupo.First().p.Nombre : grupo.First().m.Nombre,
                                        TipoComentario = string.Join(", ", grupo.Select(g => g.c.Tipo).Distinct().ToList()),
                                        FechaRegistro = grupo.First().b.FechaRegistro,
                                        Estatus = grupo.First().b.Estatus == 0 ? "Pendiente" : "Terminado",
                                        Correo = grupo.First().b.IdMedico == 0 ? grupo.First().p.Email : grupo.First().m.Email,
                                        Telefono = grupo.First().b.IdMedico == 0 ? grupo.First().p.Telefono : grupo.First().m.Telefono,
                                        Comentario = grupo.First().b.Comentario
                                    }).ToListAsync();


            if (resultados == null || resultados.Count == 0)
            {
                return NotFound(new
                {
                    estatus = "error",
                    mensaje = "No se encontraron datos.",
                    data = new List<object>()
                });
            }

            return Ok(new
            {
                estatus = "success",
                mensaje = "Datos obtenidos correctamente.",
                data = resultados
            });
        }


        [HttpPost]
        [Route("AsignarClasificacionComentarios")]
        public async Task<IActionResult> AsignarClasificacionComentarios([FromBody] ActualizarBuzonRequest request)
        {
            if (request == null || request.BuzonId <= 0 || request.ClasificacionesSeleccionadas == null || !request.ClasificacionesSeleccionadas.Any())
            {
                return BadRequest(new { estatus = "error", mensaje = "Datos inválidos." });
            }
            foreach (var clasificacionId in request.ClasificacionesSeleccionadas)
            {
                var nuevaClasificacion = new BuzonClasificacionComentario
                {
                    IdSoliditudBuzon = request.BuzonId,
                    IdClasificacionComentario = clasificacionId
                };
                await _baseDatos.BuzonClasificacionComentarios.AddAsync(nuevaClasificacion);
            }
            await _baseDatos.SaveChangesAsync();
            return Ok(new { estatus = "success", mensaje = "Clasificaciones registradas." });
        }


        [HttpPost]
        [Route("ActualizarEstatusBuzon/{idSoliditudBuzon}")]
        public async Task<IActionResult> ActualizarEstatusBuzon(int idSoliditudBuzon)
        {
            if (idSoliditudBuzon <= 0)
            {
                return BadRequest(new { estatus = "error", mensaje = "Datos inválidos." });
            }
            var buzon = await _baseDatos.Buzons.FindAsync(idSoliditudBuzon);
            if (buzon == null)
            {
                return NotFound(new { estatus = "error", mensaje = "Buzón no encontrado." });
            }
            // Cambiar a "Terminado"
            buzon.Estatus = 1;
            _baseDatos.Buzons.Update(buzon);
            await _baseDatos.SaveChangesAsync();
            return Ok(new { estatus = "success", mensaje = "Estatus actualizado." });
        }



        [HttpGet]
        [Route("GetTieneClasificacionComentario/{idSoliditudBuzon}")]
        public async Task<IActionResult> GetClasificacionComentario(int idSoliditudBuzon)
        {
            var clasificaciones = await (from cl in _baseDatos.BuzonClasificacionComentarios
                                         join c in _baseDatos.ClasificacionComentarios on cl.IdClasificacionComentario equals c.IdClasificacionComentario
                                         where cl.IdSoliditudBuzon == idSoliditudBuzon
                                         select new
                                         {
                                             cl.IdClasificacionComentario,
                                             c.Clasificacion
                                         }).ToListAsync();
            return Ok(new
            {
                estatus = "success",
                mensaje = clasificaciones.Any()
                    ? "Clasificaciones obtenidas correctamente."
                    : "No se encontraron clasificaciones para el buzón especificado.",
                data = clasificaciones
            });
        }










    }
}
