using AuthAPI.DAL;
using AuthAPI.DAL.Entities;
using AuthAPI.DAL.Storage.GetUserById;
using AuthAPI.DAL.Storage.GetUsers;
using AuthAPI.DAL.Storage.CreateUser;
using AuthAPI.DAL.Storage.UpdateUser;
using AuthAPI.DAL.Storage.RemoveUser;
using AuthAPI.Tests.DAL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthAPI.Tests.DAL;

internal static class DbHelper
{
    public static AuthDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new AuthDbContext(options);
    }

    public static User SeedUser(AuthDbContext ctx,
        string firstName = "John",
        string lastName = "Doe",
        Guid? id = null)
    {
        var appUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"{firstName}.{lastName}@test.com",
            NormalizedUserName = $"{firstName}.{lastName}@test.com".ToUpper(),
            Email = $"{firstName}.{lastName}@test.com",
            NormalizedEmail = $"{firstName}.{lastName}@test.com".ToUpper(),
            User = null!
        };

        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            ApplicationUserId = appUser.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        appUser.User = user;

        ctx.Users.Add(user);
        ctx.Set<ApplicationUser>().Add(appUser);
        ctx.SaveChanges();
        return user;
    }
}

[Collection("SqlServer")]
public class GetUserByIdStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private AuthDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;

    public GetUserByIdStorageTests(SqlServerContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = DbHelper.CreateContext(_fixture.ConnectionString);
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var storage = new GetUserByIdStorage(_context);

        var result = await storage.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsModel_WhenFound()
    {
        var user = DbHelper.SeedUser(_context, "Alice", "Smith");
        var storage = new GetUserByIdStorage(_context);

        var result = await storage.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Smith", result.LastName);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenNotFound()
    {
        var storage = new GetUserByIdStorage(_context);

        var result = await storage.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenFound()
    {
        var user = DbHelper.SeedUser(_context);
        var storage = new GetUserByIdStorage(_context);

        var result = await storage.ExistsAsync(user.Id, CancellationToken.None);

        Assert.True(result);
    }
}

[Collection("SqlServer")]
public class GetUsersStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private AuthDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;

    public GetUsersStorageTests(SqlServerContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = DbHelper.CreateContext(_fixture.ConnectionString);
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetAsync_ReturnsAll_WhenNoFilter()
    {
        DbHelper.SeedUser(_context, "Alice", "Alpha");
        DbHelper.SeedUser(_context, "Bob", "Beta");
        DbHelper.SeedUser(_context, "Carol", "Gamma");
        var storage = new GetUsersStorage(_context);

        var result = await storage.GetAsync(1, 10, null, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.Data!.Count);
    }

    [Fact]
    public async Task GetAsync_FiltersByName()
    {
        DbHelper.SeedUser(_context, "Alice", "Smith");
        DbHelper.SeedUser(_context, "Bob", "Johnson");
        DbHelper.SeedUser(_context, "Alice", "Jones");
        var storage = new GetUsersStorage(_context);

        var result = await storage.GetAsync(1, 10, "Alice", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result.Data!, u => Assert.Equal("Alice", u.FirstName));
    }

    [Fact]
    public async Task GetAsync_FiltersByLastName()
    {
        DbHelper.SeedUser(_context, "Alice", "Smith");
        DbHelper.SeedUser(_context, "Bob", "Smith");
        DbHelper.SeedUser(_context, "Carol", "Jones");
        var storage = new GetUsersStorage(_context);

        var result = await storage.GetAsync(1, 10, "Smith", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result.Data!, u => Assert.Equal("Smith", u.LastName));
    }

    [Fact]
    public async Task GetAsync_PaginatesCorrectly()
    {
        for (int i = 0; i < 5; i++)
            DbHelper.SeedUser(_context, $"User{i:D2}", "Doe");
        var storage = new GetUsersStorage(_context);

        var result = await storage.GetAsync(1, 2, null, CancellationToken.None);

        Assert.Equal(5, result.Count);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsEmpty_WhenNoMatch()
    {
        DbHelper.SeedUser(_context, "Alice", "Smith");
        var storage = new GetUsersStorage(_context);

        var result = await storage.GetAsync(1, 10, "ZZZNOMATCH", CancellationToken.None);

        Assert.Equal(0, result.Count);
        Assert.Empty(result.Data!);
    }
}

[Collection("SqlServer")]
public class UpdateUserStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private AuthDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;

    public UpdateUserStorageTests(SqlServerContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = DbHelper.CreateContext(_fixture.ConnectionString);
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task UpdateAsync_ChangesFirstAndLastName()
    {
        var user = DbHelper.SeedUser(_context, "OldFirst", "OldLast");
        var storage = new UpdateUserStorage(_context);

        await storage.UpdateAsync(user.Id, "NewFirst", "NewLast", CancellationToken.None);

        var updated = _context.Users.Find(user.Id)!;
        Assert.Equal("NewFirst", updated.FirstName);
        Assert.Equal("NewLast", updated.LastName);
        Assert.NotNull(updated.UpdatedAt);
    }
}

[Collection("SqlServer")]
public class RemoveUserStorageTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _fixture;
    private AuthDbContext _context = null!;
    private IDbContextTransaction _transaction = null!;

    public RemoveUserStorageTests(SqlServerContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _context = DbHelper.CreateContext(_fixture.ConnectionString);
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task RemoveByIdAsync_RemovesUser()
    {
        var user = DbHelper.SeedUser(_context, "ToDelete", "Bye");
        var storage = new RemoveUserStorage(_context);

        await storage.RemoveByIdAsync(user.Id, CancellationToken.None);

        Assert.False(_context.Users.Any(u => u.Id == user.Id));
    }
}
