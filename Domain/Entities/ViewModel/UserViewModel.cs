using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModel
{
    public class UserIndexModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool LockoutForever { get; set; }
        public string Roles { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class UserSearch
    {
        public string? keyword { get; set; }
        public bool? isActive { get; set; }
        public string? role { get; set; }
        public int currentPage { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public int skip { get; set; } = 0;
    }
}
