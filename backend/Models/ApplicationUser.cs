using Microsoft.AspNetCore.Identity;

namespace barbearia.api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public virtual Address Address { get; set; }

    }
}
