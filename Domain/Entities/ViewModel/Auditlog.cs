using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModel
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = "";
        public string AdminName { get; set; } = "";
        public string Action { get; set; } = "";   // Created / Updated / Deleted
        public string EntityName { get; set; } = "";   // Product / Order / Review ...
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }         // JSON
        public string? NewValues { get; set; }         // JSON
        public string? IpAddress { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

}
