using Microsoft.AspNetCore.Identity;

namespace AlumniHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        // "Student" or "Alumni"
        public string Role { get; set; } = string.Empty;
    }
}