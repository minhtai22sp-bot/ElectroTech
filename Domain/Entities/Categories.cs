using AspNetCoreHero.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Categories : AuditableEntity
    {
        [MaxLength(100)]
        public string Name { get; set; } 

        [MaxLength(100)]
        public string Slug { get; set; } 

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int? ParentId { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public virtual ICollection<Product> Products { get; set; }
    }
}
