using Microsoft.AspNetCore.Identity;

namespace SlipzyD2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
