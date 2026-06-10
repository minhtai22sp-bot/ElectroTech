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
    public class OrderItem : AuditableEntity
    {
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
      
        [MaxLength(200)]
        public string ProductName { get; set; }
        [MaxLength(50)]
        public string ProductCode { get; set; }
        [MaxLength(500)]
        public string ProductImage { get; set; } 

        [MaxLength(200)]
        public string ProductSlug { get; set; } 

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
