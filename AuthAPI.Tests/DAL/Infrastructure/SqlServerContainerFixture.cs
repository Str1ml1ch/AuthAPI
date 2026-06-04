using AuthAPI.DAL;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AuthAPI.Tests.DAL.Infrastructure;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context = new AuthDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
