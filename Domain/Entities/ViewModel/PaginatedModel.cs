using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModel
{
    public class PaginatedModel
    {
        public int pageSize { get; set; } = 50;
        public int currentPage { get; set; }
        public int skip { get; set; }
    }
}
