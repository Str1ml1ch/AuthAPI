using AuthAPI.DAL;
using AuthAPI.DAL.Entities;
using AuthAPI.DAL.Storage.GetUserById;
using AuthAPI.DAL.Storage.GetUsers;
using AuthAPI.DAL.Storage.CreateUser;
using AuthAPI.DAL.Storage.UpdateUser;
using AuthAPI.DAL.Storage.RemoveUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Tests.DAL;

internal static class DbHelper
{
    public static AuthDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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

public class GetUserByIdStorageTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        using var ctx = DbHelper.CreateContext();
        var storage = new GetUserByIdStorage(ctx);

        var result = await storage.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsModel_WhenFound()
    {
        using var ctx = DbHelper.CreateContext();
        var user = DbHelper.SeedUser(ctx, "Alice", "Smith");
        var storage = new GetUserByIdStorage(ctx);

        var result = await storage.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Smith", result.LastName);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenNotFound()
    {
        using var ctx = DbHelper.CreateContext();
        var storage = new GetUserByIdStorage(ctx);

        var result = await storage.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenFound()
    {
        using var ctx = DbHelper.CreateContext();
        var user = DbHelper.SeedUser(ctx);
        var storage = new GetUserByIdStorage(ctx);

        var result = await storage.ExistsAsync(user.Id, CancellationToken.None);

        Assert.True(result);
    }
}

public class GetUsersStorageTests
{
    [Fact]
    public async Task GetAsync_ReturnsAll_WhenNoFilter()
    {
        using var ctx = DbHelper.CreateContext();
        DbHelper.SeedUser(ctx, "Alice", "Alpha");
        DbHelper.SeedUser(ctx, "Bob", "Beta");
        DbHelper.SeedUser(ctx, "Carol", "Gamma");
        var storage = new GetUsersStorage(ctx);

        var result = await storage.GetAsync(1, 10, null, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.Data!.Count);
    }

    [Fact]
    public async Task GetAsync_FiltersByName()
    {
        using var ctx = DbHelper.CreateContext();
        DbHelper.SeedUser(ctx, "Alice", "Smith");
        DbHelper.SeedUser(ctx, "Bob", "Johnson");
        DbHelper.SeedUser(ctx, "Alice", "Jones");
        var storage = new GetUsersStorage(ctx);

        var result = await storage.GetAsync(1, 10, "Alice", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result.Data!, u => Assert.Equal("Alice", u.FirstName));
    }

    [Fact]
    public async Task GetAsync_FiltersByLastName()
    {
        using var ctx = DbHelper.CreateContext();
        DbHelper.SeedUser(ctx, "Alice", "Smith");
        DbHelper.SeedUser(ctx, "Bob", "Smith");
        DbHelper.SeedUser(ctx, "Carol", "Jones");
        var storage = new GetUsersStorage(ctx);

        var result = await storage.GetAsync(1, 10, "Smith", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result.Data!, u => Assert.Equal("Smith", u.LastName));
    }

    [Fact]
    public async Task GetAsync_PaginatesCorrectly()
    {
        using var ctx = DbHelper.CreateContext();
        for (int i = 0; i < 5; i++)
            DbHelper.SeedUser(ctx, $"User{i:D2}", "Doe");
        var storage = new GetUsersStorage(ctx);

        var result = await storage.GetAsync(1, 2, null, CancellationToken.None);

        Assert.Equal(5, result.Count);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetAsync_ReturnsEmpty_WhenNoMatch()
    {
        using var ctx = DbHelper.CreateContext();
        DbHelper.SeedUser(ctx, "Alice", "Smith");
        var storage = new GetUsersStorage(ctx);

        var result = await storage.GetAsync(1, 10, "ZZZNOMATCH", CancellationToken.None);

        Assert.Equal(0, result.Count);
        Assert.Empty(result.Data!);
    }
}

public class UpdateUserStorageTests
{
    [Fact]
    public async Task UpdateAsync_ChangesFirstAndLastName()
    {
        using var ctx = DbHelper.CreateContext();
        var user = DbHelper.SeedUser(ctx, "OldFirst", "OldLast");
        var storage = new UpdateUserStorage(ctx);

        await storage.UpdateAsync(user.Id, "NewFirst", "NewLast", CancellationToken.None);

        var updated = ctx.Users.Find(user.Id)!;
        Assert.Equal("NewFirst", updated.FirstName);
        Assert.Equal("NewLast", updated.LastName);
        Assert.NotNull(updated.UpdatedAt);
    }
}

public class RemoveUserStorageTests
{
    [Fact]
    public async Task RemoveByIdAsync_RemovesUser()
    {
        using var ctx = DbHelper.CreateContext();
        var user = DbHelper.SeedUser(ctx, "ToDelete", "Bye");
        var storage = new RemoveUserStorage(ctx);

        await storage.RemoveByIdAsync(user.Id, CancellationToken.None);

        Assert.False(ctx.Users.Any(u => u.Id == user.Id));
    }
}
