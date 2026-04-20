using AuthAPI.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Storage.GetUserById
{
    public class GetUserByIdStorage : IGetUserByIdStorage
    {
        private readonly AuthDbContext _context;

        public GetUserByIdStorage(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        {
            return await _context.Users.AnyAsync(u => u.Id == id, ct);
        }
    }
}
