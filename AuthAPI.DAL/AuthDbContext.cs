using AuthAPI.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.DAL
{
    public class AuthDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        string,
        IdentityUserClaim<string>,
        ApplicationUserRole,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>
    {
        public DbSet<User> Users { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(au => au.User)
                      .WithOne(u => u.ApplicationUser)
                      .HasForeignKey<User>(u => u.Id);

                entity.HasMany(au => au.UserRoles)
                      .WithOne(ur => ur.User)
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.HasMany(ar => ar.UserRoles)
                      .WithOne(ur => ur.Role)
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired();
            });

            modelBuilder.Entity<ApplicationUserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.HasIndex(u => u.ApplicationUserId);

                entity.HasOne(u => u.ApplicationUser)
                        .WithOne(au => au.User)
                        .HasForeignKey<User>(u => u.ApplicationUserId)
                        .IsRequired();

                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            });
        }
    }
}
