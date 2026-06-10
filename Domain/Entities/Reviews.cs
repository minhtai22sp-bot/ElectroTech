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
    public class Review : AuditableEntity
    {
        public int ProductId { get; set; }
        public Guid UserId { get; set; }
        public int? OrderItemId { get; set; }

        [Range(1, 5)]
        public byte Rating { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; } 
        public bool IsApproved { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
