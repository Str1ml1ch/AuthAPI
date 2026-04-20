using Shared.DAL.Entities;

namespace AuthAPI.DAL.Entities
{
    public class User : BaseDbEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string ApplicationUserId { get; set; } = null!;

        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
