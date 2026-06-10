using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] ProfilePicture { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; } = false;
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool LockoutForever { get; set; }
        public bool IsStoreOwner { get; set; }
    }
    public class ApplicationRole : IdentityRole<string>
    {
        public ApplicationRole() { }
        public ApplicationRole(string roleName) : this()
        {
            Id = Guid.NewGuid().ToString();
            Name = roleName;
        }
        [StringLength(30)]
        public string Code { get; set; }
      
    }
}
