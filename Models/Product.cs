using System;
using System.Collections.Generic;

namespace MediFinder_Backend.Models;

public partial class Product
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public decimal? Rating { get; set; }

    public int? Stock { get; set; }

    public string? BrandId { get; set; }

    public string? CategoryId { get; set; }

    public string? Thumbnail { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
