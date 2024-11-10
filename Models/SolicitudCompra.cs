using MediFinder_Backend.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("solicitudCompra", Schema = "dbo")]  // Especificar el esquema dbo
public partial class SolicitudCompra
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public int? Amount { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Total { get; set; }

    public virtual Product? Product { get; set; }
}
