using System;

namespace ApiEcommerce.Models.Dtos;

public class UpdatedProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Stock { get; set; }
    public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    public int CategoryId { get; set; }
}
