using Microsoft.AspNetCore.Identity;
using System;

namespace KargoTakip.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}