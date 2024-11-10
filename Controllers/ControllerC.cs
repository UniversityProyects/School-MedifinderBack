using MediFinder_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using MediFinder_Backend.Models;  // Asegúrate de que este using esté presente

namespace MediFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerC : ControllerBase
    {
        private readonly MedifinderContext _baseDatos;

        public ControllerC(MedifinderContext baseDatos)
        {
            _baseDatos = baseDatos;
        }

        // Endpoint para sumar números (solo un ejemplo)
        [HttpGet]
        [Route("sumarNumeros/{numero}")]
        public IActionResult SumarNumeros(int numero)
        {
            int resultado = numero + numero;
            return Ok(new { resultado });
        }

        // Endpoint 1: /api/items?q=:query para buscar productos
        [HttpGet]
        [Route("items")]
        public IActionResult GetItems([FromQuery] string q)
        {
            var items = _baseDatos.Products
                                  .Where(p => p.Title.Contains(q) || p.Description.Contains(q))
                                  .Select(p => new
                                  {
                                      p.Id,
                                      p.Title,
                                      p.Price,
                                      p.Stock,
                                      p.Thumbnail
                                  })
                                  .ToList();

            return Ok(items);
        }

        // Endpoint 2: /api/items/:id para obtener detalles de un producto específico
        [HttpGet]
        [Route("items/{id}")]
        public IActionResult GetItemById(int id)
        {
            var product = _baseDatos.Products
                                    .Where(p => p.Id == id)
                                    .Select(p => new
                                    {
                                        p.Id,
                                        p.Title,
                                        p.Description,
                                        p.Price,
                                        p.Stock,
                                        p.Thumbnail,
                                        Images = _baseDatos.ProductImages
                                                           .Where(img => img.ProductId == p.Id)
                                                           .Select(img => img.ImageUrl)
                                    })
                                    .FirstOrDefault();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // Endpoint 3: /api/addSale para registrar una compra
        [HttpPost]
        [Route("addSale")]
        public IActionResult AddSale([FromBody] PurchaseRequest request)
        {
            var product = _baseDatos.Products.Find(request.ProductId);

            if (product == null || product.Stock < request.Amount)
                return BadRequest(new { success = false, message = "Producto no disponible o stock insuficiente." });

            var total = product.Price * request.Amount;

            // Crear una nueva venta
            var purchase = new SolicitudCompra
            {
                ProductId = request.ProductId,
                Amount = request.Amount,
                PurchaseDate = DateTime.Now,
                UnitPrice = product.Price,
                Total = total
            };

            _baseDatos.SolicitudCompra.Add(purchase);
            _baseDatos.SaveChanges();

            // Actualizar el stock del producto
            product.Stock -= request.Amount;
            _baseDatos.SaveChanges();

            return Ok(new { success = true });
        }

        // Endpoint 4: /api/sales para obtener todas las compras registradas
        [HttpGet]
        [Route("sales")]
        public IActionResult GetSales()
        {
            var sales = _baseDatos.SolicitudCompra
                                  .Select(s => new
                                  {
                                      s.Id,
                                      s.ProductId,
                                      s.Amount,
                                      s.PurchaseDate,
                                      s.UnitPrice,
                                      s.Total
                                  })
                                  .ToList();

            return Ok(sales);
        }

        // Clase de solicitud de compra
        public class PurchaseRequest
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }
    }
}
