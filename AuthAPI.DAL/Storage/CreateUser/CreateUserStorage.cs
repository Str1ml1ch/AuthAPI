using AuthAPI.DAL.Entities;

namespace AuthAPI.DAL.Storage.CreateUser
{
    public class CreateUserStorage : ICreateUserStorage
    {
        private readonly AuthDbContext _context;

        public CreateUserStorage(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(string firstName, string lastName, CancellationToken ct)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            return user.Id;
        }
    }
}
