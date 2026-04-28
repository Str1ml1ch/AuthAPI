using AuthAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAPI.DAL.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> entity)
        {
            entity.HasOne(au => au.User)
                  .WithOne(u => u.ApplicationUser)
                  .HasForeignKey<User>(u => u.Id);

            entity.HasMany(au => au.UserRoles)
                  .WithOne(ur => ur.User)
                  .HasForeignKey(ur => ur.UserId)
                  .IsRequired();
        }
    }
}
