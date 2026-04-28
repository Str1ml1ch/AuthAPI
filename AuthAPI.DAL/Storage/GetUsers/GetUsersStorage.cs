using AuthAPI.Core.Models;
using AuthAPI.DAL.Specifications.Users;
using Homework.Ticketing.System.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL.Storage.GetUsers
{
    public class GetUsersStorage : IGetUsersStorage
    {
        private readonly AuthDbContext _context;

        public GetUsersStorage(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<List<UserModel>>> GetAsync(
            int page,
            int pageSize,
            string? searchName,
            CancellationToken ct)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(new UserByNameSpecification(searchName).ToExpression());

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync(ct);

            return new ResultModel<List<UserModel>> { Data = items, Count = totalCount };
        }
    }
}
