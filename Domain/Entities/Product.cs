using AspNetCoreHero.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Product : AuditableEntity
    {

        public int CategoryId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Slug { get; set; }
        [MaxLength(50)]
        public string Code { get; set; } 

        public string Description { get; set; }

        [MaxLength(100)]
        public string Brand { get; set; }

        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Stock { get; set; }

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        [ForeignKey("CategoryId")]
        public virtual Categories Categories { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<ProductSpec> ProductSpecs { get; set; }
    }
}
