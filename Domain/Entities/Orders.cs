using AspNetCoreHero.Abstractions.Domain;
using Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
  

    public class Order : AuditableEntity
    {
        [MaxLength(20)]
        public string OrderCode { get; set; } 
        public int? CustomerId { get; set; }
        public Guid? UserId { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; } 

        [MaxLength(256)]
        public string Email { get; set; } 

        [MaxLength(20)]
        public string Phone { get; set; } 

        [MaxLength(500)]
        public string ShippingAddress { get; set; } 

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }

        public int? CouponId { get; set; }

        [MaxLength(50)]
        public string? CouponCode { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

      
        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(500)]
        public string? AdminNote { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
