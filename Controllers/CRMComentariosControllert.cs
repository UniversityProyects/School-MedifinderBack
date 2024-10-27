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
        [Route("GetClasificacionesComentarios")]
        public async Task<ActionResult<IEnumerable<ClasificacionComentario>>> GetClasificacionesComentarios()
        {
            var clasificacionesComentarios = await _baseDatos.ClasificacionComentarios.ToListAsync();
            return Ok(clasificacionesComentarios);
        }

    }
}
