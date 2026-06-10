using AspNetCoreHero.Abstractions.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

public class CartItem : AuditableEntity
{
    [NotMapped]
    public new object? SortOrder { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public int ProductId { get; set; }

    [MaxLength(200)]
    public string ProductName { get; set; } = "";

    public decimal Price { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; } = "";

    public int Quantity { get; set; }

    [NotMapped]
    public decimal Subtotal => Price * Quantity;

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
}