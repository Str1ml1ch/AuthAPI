using Microsoft.AspNetCore.Identity;

namespace AuthAPI.DAL.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<ApplicationUserRole> UserRoles { get; set; } = null!;
    }
}
