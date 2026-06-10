using AspNetCoreHero.Abstractions.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    public class ProductSpec : AuditableEntity
    {
        public int ProductId { get; set; }

        [MaxLength(100)]
        public string SpecKey { get; set; }

        [MaxLength(200)]
        public string SpecValue { get; set; }

        [MaxLength(100)]
        public string? GroupName { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

       
    }
}