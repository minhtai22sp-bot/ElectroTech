using AspNetCoreHero.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Customer : AuditableEntity
    {
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(500)]
        public string Name { get; set; }
        public string Address { get; set; }
        [MaxLength(200)]
        public string Note { get; set; }
        [MaxLength(50)]
        public string Email { get; set; }
        [MaxLength(50)]
        public string Phone { get; set; }
    }
}
