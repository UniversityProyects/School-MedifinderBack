using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediFinder_Backend.Controllers
{
    [Route("api/Tiendita")]
    [ApiController]
    public class ControllerJ : Controller
    {
        private readonly MedifinderContext _baseDatos;

        public ControllerJ(MedifinderContext baseDatos)
        {
            this._baseDatos = baseDatos;
        }

        // itemms : Obtiene la lista de productos ------------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        [Route("items")]
        public IActionResult items([FromQuery] string search)
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
            if (productos.Count == 0)
            {
                return Ok(new
                {
                    mensaje = "No se encontraron productos.",
                    estatus = "error",
                    data = new { TotalResults = 0, Products = new object[] { } }
                });
            }

            return Ok(new
            {
                mensaje = "Productos encontrados exitosamente.",
                estatus = "success",
                data = new
                {
                    TotalResults = productos.Count(),
                    Products = productos
                }
            });
        }

        // item/id : Permite obtener los detalles por producto -------------------------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("item/{id}")]
        public IActionResult detallesProductos(int id)
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


        //addSale : Permite realizar una compra -----------------------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("addSale")]
        public async Task<IActionResult> addSale([FromBody] DTOCompra dtoCompra)
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
            DateTime fechaCompra = DateTime.UtcNow.ToLocalTime();

            var nuevaCompra = new ComprasUsuario
            {
                ProductId = dtoCompra.ProductId,
                Amount = dtoCompra.Amount,
                PurchaseDate = fechaCompra,
                UnitPrice = producto.Price,
                Total = total
            };

            try
            {
                _baseDatos.ComprasUsuarios.Add(nuevaCompra);
                await _baseDatos.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Compra registrada.",
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

        //sales: permite consultar las compras realizadas -----------------------------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("sales")]
        public async Task<IActionResult> sales()
        {
            try
            {
                var compras = await (from c in _baseDatos.ComprasUsuarios
                                     join p in _baseDatos.Products on c.ProductId equals p.Id
                                     select new
                                     {
                                         Id = c.Id,
                                         IdProducto = c.ProductId,
                                         Cantidad = c.Amount,
                                         Fecha = c.PurchaseDate.HasValue ? c.PurchaseDate.Value.ToString("dd/MM/yyyy") : null,
                                         Costo = c.UnitPrice,
                                         TotalCompra =  c.Total,
                                         NombreProducto = p.Title
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
                    mensaje = "Compras obtenidas exitosamente.",
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

        public class DTOCompra
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }


    }
}
