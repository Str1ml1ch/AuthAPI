using AuthAPI.DAL.Storage.CreateUser;
using AuthAPI.DAL.Storage.GetUserById;
using AuthAPI.DAL.Storage.GetUsers;
using AuthAPI.DAL.Storage.RemoveUser;
using AuthAPI.DAL.Storage.UpdateUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.DAL.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection services, string connectionString)
        {

            return services.AddDbContextPool<AuthDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddScoped<ICreateUserStorage, CreateUserStorage>()
                .AddScoped<IGetUserByIdStorage, GetUserByIdStorage>()
                .AddScoped<IGetUsersStorage, GetUsersStorage>()
                .AddScoped<IUpdateUserStorage, UpdateUserStorage>()
                .AddScoped<IRemoveUserStorage, RemoveUserStorage>();
        }
    }
}
