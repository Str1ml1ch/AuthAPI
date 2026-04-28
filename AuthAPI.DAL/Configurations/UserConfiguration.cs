using AuthAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAPI.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(u => u.Id);

            entity.HasIndex(u => u.ApplicationUserId);

            entity.HasOne(u => u.ApplicationUser)
                  .WithOne(au => au.User)
                  .HasForeignKey<User>(u => u.ApplicationUserId)
                  .IsRequired();

            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        }
    }
}
