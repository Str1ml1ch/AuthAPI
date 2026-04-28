using AuthAPI.DAL.Entities;
using AuthAPI.DAL.Storage.CreateUser;
using AuthAPI.DAL.Storage.GetUserById;
using AuthAPI.DAL.Storage.GetUsers;
using AuthAPI.DAL.Storage.RemoveUser;
using AuthAPI.DAL.Storage.UpdateUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthAPI.DAL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<AuthDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<AuthDbContext>();

            return services;
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
