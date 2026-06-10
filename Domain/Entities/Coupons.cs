using AspNetCoreHero.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }

    public class Coupon : AuditableEntity
    {
        [MaxLength(50)]
        public string Code { get; set; } 

        [MaxLength(200)]
        public string Name { get; set; } 

        [MaxLength(500)]
        public string? Description { get; set; }

      
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }

        public int MaxUsage { get; set; }
        public int UsedCount { get; set; }

     
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;


    }
}
