using Microsoft.AspNetCore.Identity;

namespace AuthAPI.DAL.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required User User { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; } = null!;
    }
}
