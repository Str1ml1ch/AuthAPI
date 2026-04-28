using AuthAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAPI.DAL.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> entity)
        {
            entity.HasMany(ar => ar.UserRoles)
                  .WithOne(ur => ur.Role)
                  .HasForeignKey(ur => ur.RoleId)
                  .IsRequired();
        }
    }
}
