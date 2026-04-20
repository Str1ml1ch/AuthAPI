using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Storage.RemoveUser
{
    public class RemoveUserStorage : IRemoveUserStorage
    {
        private readonly AuthDbContext _context;

        public RemoveUserStorage(AuthDbContext context)
        {
            _context = context;
        }

        public async Task RemoveByIdAsync(Guid id, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

            _context.Users.Remove(user!);
            await _context.SaveChangesAsync(ct);
        }
    }
}
