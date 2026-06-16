using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class TopProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public int SoldCount { get; set; }
        public decimal Revenue { get; set; }
    }

}
