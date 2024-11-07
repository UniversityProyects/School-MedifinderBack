using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

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
