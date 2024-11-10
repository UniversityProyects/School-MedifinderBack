using MediFinder_Backend.ModelosEspeciales;
using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.ModelosEspeciales.AddSale;

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerD : Controller
    {
        private readonly MedifinderContext _baseDatos;

        public ControllerD(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        [HttpGet]
        [Route("sumarNumeros/{numero}")]
        public IActionResult SumarNumeros(int numero)
        {
            int resultado = numero + numero;
            return Ok(new { resultado });
        }

        [HttpGet]
        [Route("items")]
        public async Task<IActionResult> GetItems(string q)
        {
            var query = await (from p in _baseDatos.Products
                               join b in _baseDatos.Brands on p.BrandId equals b.Id.ToString()
                               join c in _baseDatos.Categories on p.CategoryId equals c.Id.ToString()
                               where p.Title.Contains(q)
                               select new
                               {
                                   p.Id,
                                   p.Title,
                                   p.Description,
                                   p.Price,
                                   p.DiscountPercentage,
                                   p.Rating,
                                   p.Stock,
                                   Brand = b.Name,
                                   Category = c.Name,
                                   p.CategoryId,
                                   p.BrandId,
                                   Thumbnail = p.Thumbnail
                               }).ToListAsync();



            if (query.Count == 0)
            {
                return NotFound(new { message = "No se encontraron productos" });
            }

            return Ok(query);
        }

        [HttpGet]
        [Route("items/{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var images = await (from pi in _baseDatos.ProductImages
                                where pi.ProductId == id
                                select pi.ImageUrl).ToListAsync();

            var item = await (from p in _baseDatos.Products
                              join b in _baseDatos.Brands on p.BrandId equals b.Id.ToString()
                              join c in _baseDatos.Categories on p.CategoryId equals c.Id.ToString()
                              where p.Id == id
                              select new
                              {
                                  p.Id,
                                  p.Title,
                                  p.Description,
                                  p.Price,
                                  p.DiscountPercentage,
                                  p.Rating,
                                  p.Stock,
                                  Brand = b.Name,
                                  Category = c.Name,
                                  p.BrandId,
                                  p.CategoryId,
                                  Thumbnail = p.Thumbnail,
                                  Images = images
                              }).FirstOrDefaultAsync();



            if (item == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            return Ok(item);
        }

        [HttpPost]
        [Route("addSale")]
        public async Task<ActionResult> addSale([FromBody] AddSaleDTO sale)
        {
            //Valida que el modelo recibido este correcto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existeProducto = await _baseDatos.Products.FirstOrDefaultAsync(a => a.Id == sale.ProductId);
                if (existeProducto == null)
                {
                    return NotFound(new {message = "No existe el producto", status = false});
                }

                if (sale.Quantity <= 0)
                {
                    return BadRequest(new {message = "No se pueden registrar compras con cantidades iguales o menores a 0", status = false});
                }

                if (sale.Quantity > existeProducto.Stock)
                {
                    return StatusCode(403, new {message = "No hay suficientes productos", status = false});
                }

                Console.WriteLine((sale.Quantity * sale.UnitPrice) * (1 - sale.discountPercentage / 100));

                //Formateamos el modelo de la cita
                var newSale = new Purchase
                {
                    ProductId = sale.ProductId,
                    Quantity = sale.Quantity,
                    PurchaseDate = DateTime.Now,
                    UnitPrice = sale.UnitPrice,
                    Total = sale.Quantity * sale.UnitPrice,
                    discountPercentage = sale.discountPercentage,
                    totalWithDiscount = (sale.Quantity * sale.UnitPrice) * (1 - sale.discountPercentage / 100)
                };

                existeProducto.Stock -= sale.Quantity;

                // Guardar la cita en la base de datos
                _baseDatos.Purchases.Add(newSale);
                await _baseDatos.SaveChangesAsync();

                return Ok(new { message = "Compra registrada correctamente", newSale.Id, status = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}", status = false });
            }
        }

        [HttpGet]
        [Route("sales")]
        public async Task<IActionResult> getPurchases()
        {
            var result = from pu in _baseDatos.Purchases
                         join pr in _baseDatos.Products on pu.ProductId equals pr.Id
                         join c in _baseDatos.Categories on pr.CategoryId equals c.Id.ToString()
                         join b in _baseDatos.Brands on pr.BrandId equals b.Id.ToString()
                         select new
                         {
                             pu.Id,
                             pu.ProductId,
                             ProductTitle = pr.Title,
                             ProductDescription = pr.Description,
                             ProductThumbnail = pr.Thumbnail,
                             pr.CategoryId,
                             Category = c.Name,
                             pr.BrandId,
                             Brand = b.Name,
                             pu.Quantity,
                             pu.PurchaseDate,
                             pu.UnitPrice,
                             pu.Total,
                             pu.discountPercentage,
                             pu.totalWithDiscount
                         };


            if (result == null)
            {
                return NotFound(new { message = "No hay comprass" });
            }

            return Ok(result);
        }
    }
}
