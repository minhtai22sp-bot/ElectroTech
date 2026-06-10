using AspNetCoreHero.Abstractions.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities;

public class ProductImage : AuditableEntity
{
    public int ProductId { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; }

    [MaxLength(200)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }

}