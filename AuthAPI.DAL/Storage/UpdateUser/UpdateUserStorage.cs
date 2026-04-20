using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Storage.UpdateUser
{
    public class UpdateUserStorage : IUpdateUserStorage
    {
        private readonly AuthDbContext _context;

        public UpdateUserStorage(AuthDbContext context)
        {
            _context = context;
        }

        public async Task UpdateAsync(Guid id, string firstName, string lastName, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

            user!.FirstName = firstName;
            user.LastName = lastName;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }
}
