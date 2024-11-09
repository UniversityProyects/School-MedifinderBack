using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static MediFinder_Backend.Controllers.ControllerJ;

namespace MediFinder_Backend.Controllers
{
    [Route("api/AppTienda")]
    [ApiController]
    public class ControllerE : Controller
    {
        private readonly MedifinderContext _baseDatos;

        public ControllerE(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        //BUSQUEDA PRODUCTO
        [HttpGet]
        [Route("items")]
        public IActionResult SearchProducts([FromQuery] string search)
        {
            var productos = _baseDatos.Products
                .Where(p => (p.Title.Contains(search)) && p.CategoryId != null)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    Category = _baseDatos.Categories
                                 .Where(c => c.Id.ToString() == p.CategoryId)
                                 .Select(c => c.Name)
                                 .FirstOrDefault(),
                    p.Price,
                    p.Rating,
                    Images = p.ProductImages.Select(img => img.ImageUrl).ToList()
                })
                .ToList();

            return Ok(new
            {
                TotalResults = productos.Count(),
                Products = productos
            });
        }


        // DETALLE PRODUCTO
        [HttpGet]
        [Route("item/{id}")]
        public IActionResult GetProductDetails(int id)
        {
            var producto = _baseDatos.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.Price,
                    p.DiscountPercentage,
                    p.Rating,
                    p.Stock,
                    p.Thumbnail,
                    Brand = _baseDatos.Brands
                            .Where(b => b.Id.ToString() == p.BrandId)
                            .Select(b => b.Name)
                            .FirstOrDefault(),
                    Category = _baseDatos.Categories
                                .Where(c => c.Id.ToString() == p.CategoryId)
                                .Select(c => c.Name)
                                .FirstOrDefault(),
                    Images = p.ProductImages.Select(img => img.ImageUrl).ToList()
                })
                .FirstOrDefault();

            if (producto == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            return Ok(producto);
        }

        //REGISTRAR COMPRA
        [HttpPost]
        [Route("addSale")]
        public async Task<IActionResult> addSale([FromBody] DTOCompras dtoCompra)
        {
            if (dtoCompra == null)
            {
                return BadRequest(new
                {
                    mensaje = "La compra no puede ser nula.",
                    estatus = "error",
                    data = new object[] { }
                });
            }

            var producto = await _baseDatos.Products.FindAsync(dtoCompra.ProductId);
            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = $"El producto con ID {dtoCompra.ProductId} no existe.",
                    estatus = "error",
                    data = new object[] { }
                });
            }

            decimal total = (producto.Price ?? 0) * dtoCompra.Amount;
            DateTime fechaCompra = DateTime.UtcNow;

            var nuevaCompra = new Compra
            {
                ProductId = dtoCompra.ProductId,
                Amount = dtoCompra.Amount,
                PurchaseDate = fechaCompra,
                UnitPrice = producto.Price,
                Total = total
            };

            try
            {
                _baseDatos.Compras.Add(nuevaCompra);
                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Compra registrada exitosamente.",
                    estatus = "success",
                    data = new
                    {
                        id = nuevaCompra.Id,
                        productId = nuevaCompra.ProductId,
                        amount = nuevaCompra.Amount,
                        purchaseDate = nuevaCompra.PurchaseDate,
                        unitPrice = nuevaCompra.UnitPrice,
                        total = nuevaCompra.Total
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"Error al registrar la compra: {ex.Message}",
                    estatus = "error",
                    data = new object[] { }
                });
            }

        }

        public class DTOCompras
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }

        // LISTA COMPRAS
        [HttpGet]
        [Route("sales")]
        public async Task<IActionResult> sales()
        {
            try
            {
                var compras = await (from c in _baseDatos.Compras
                                     join p in _baseDatos.Products on c.ProductId equals p.Id
                                     select new
                                     {
                                         c.Id,
                                         c.ProductId,
                                         c.Amount,
                                         c.PurchaseDate,
                                         c.UnitPrice,
                                         c.Total,
                                         ProductName = p.Title
                                     }).ToListAsync();

                if (compras == null)
                {
                    return Ok(new
                    {
                        mensaje = "No se encontraron compras.",
                        estatus = "error",
                        data = new object[] { }
                    });
                }

                return Ok(new
                {
                    mensaje = "Listado de compras obtenido exitosamente.",
                    estatus = "success",
                    data = compras
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = $"Error al obtener las compras: {ex.Message}",
                    estatus = "error",
                    data = new object[] { }
                });
            }
        }


    }
}
