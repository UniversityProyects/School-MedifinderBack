using MediFinder_Backend.ModelosEspeciales;
using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.BuzonRequest;

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
            var tiposComentarios = await _baseDatos.TipoComentarios
                .Where(tc => tc.Estatus == 1)
                .ToListAsync();

            return Ok(tiposComentarios);
        }


        [HttpGet]
        [Route("GetClasificacionesComentarios")]
        public async Task<ActionResult<IEnumerable<ClasificacionComentario>>> GetClasificacionesComentarios()
        {
            var clasificacionesComentarios = await _baseDatos.ClasificacionComentarios.ToListAsync();
            return Ok(clasificacionesComentarios);
        }

        /*-------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("InsertSolicitudBuzonConTipos")]
        public async Task<IActionResult> InsertSolicitudBuzonConTipos([FromBody] BuzonRequest.BuzonRequestDto request)
        {
            if (request == null || !request.TipoComentariosIds.Any())
            {
                return BadRequest("La solicitud de buzon o la lista de tipos de comentario no es válida.");
            }

            try
            {

                var buzon = new Buzon
                {
                    IdMedico = request.IdMedico ?? 0,
                    IdPaciente = request.IdPaciente ?? 0,
                    Comentario = request.Comentario,
                    FechaRegistro = DateOnly.FromDateTime(DateTime.Now),
                    Estatus = 0
                };

                _baseDatos.Buzons.Add(buzon);
                await _baseDatos.SaveChangesAsync();

                var buzonTipoComentarios = request.TipoComentariosIds.Select(tipoId => new BuzonTipoComentario
                {
                    IdSoliditudBuzon = buzon.IdSoliditudBuzon,
                    IdTipoComentario = tipoId
                }).ToList();

                _baseDatos.BuzonTipoComentarios.AddRange(buzonTipoComentarios);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Solicitud de Buzon insertada correctamente", buzonId = buzon.IdSoliditudBuzon });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        /*----------------------------------------------------------------------------------*/
        [HttpGet]
        [Route("ObtenerSolicitudesBuzon")]
        public async Task<IActionResult> ObtenerSolicitudesBuzon(int? idMedico = null, int? idPaciente = null)
        {
            
            if (idMedico == null && idPaciente == null)
            {
                return BadRequest("Se debe proporcionar un IdMedico o un IdPaciente.");
            }

            var solicitudes = await _baseDatos.Buzons
                .Include(b => b.BuzonTipoComentarios)
                .Include(b => b.BuzonClasificacionComentarios)
                .Where(b =>
                    (idMedico != null && b.IdMedico == idMedico) ||
                    (idPaciente != null && b.IdPaciente == idPaciente))
                .OrderByDescending(b => b.IdSoliditudBuzon)
                .Select(b => new SolicitudBuzonResponseDto
                {
                    IdSolicitudBuzon = b.IdSoliditudBuzon,
                    IdMedico = b.IdMedico,
                    IdPaciente = b.IdPaciente,
                    Comentario = b.Comentario,
                    FechaRegistro = b.FechaRegistro.ToDateTime(TimeOnly.MinValue),
                    FechaModificacion = b.FechaModificacion.HasValue ? b.FechaModificacion.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Estatus = b.Estatus?? 0,
                    TiposComentarios = b.BuzonTipoComentarios.Select(t => new TipoComentarioDto
                    {
                        IdTipoComentario = t.IdTipoComentario,
                        Nombre = t.IdTipoComentarioNavigation.Tipo
                    }).ToList(),
                    ClasificacionesComentarios = b.BuzonClasificacionComentarios.Select(c => new ClasificacionComentarioDto
                    {
                        IdClasificacionComentario = c.IdClasificacionComentario,
                        Nombre = c.IdClasificacionComentarioNavigation.Clasificacion
                    }).ToList()
                })
                .ToListAsync();

            if (solicitudes == null || !solicitudes.Any())
            {
                return NotFound("No se encontraron solicitudes de buzón.");
            }

            return Ok(solicitudes);
        }


    }
}
