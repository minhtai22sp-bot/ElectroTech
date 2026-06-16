using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModel
{
    public class ProductSidebarStats
    {
        public int TotalCount { get; set; }
        public int InStockCount { get; set; }
        public int OnSaleCount { get; set; }
        public int FeaturedCount { get; set; }
        public List<CategoryCount> CategoryCounts { get; set; } = new();
        public List<BrandCount> BrandCounts { get; set; } = new();
    }

    public class CategoryCount
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    public class BrandCount
    {
        public string Brand { get; set; } = "";
        public int Count { get; set; }
    }
}
