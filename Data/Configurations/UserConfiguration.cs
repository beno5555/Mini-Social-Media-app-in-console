using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Models;

namespace social_media_console_app.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User", table => table.HasCheckConstraint("CK_User_DateOfBirth", 
            "DATEDIFF(year, DateOfBirth, GETUTCDATE()) BETWEEN 13 AND 100") );
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Username)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.DateOfBirth)
            .IsRequired();

        builder.HasIndex(user => user.Username).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(user => user.PasswordSalt)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.Bio).HasMaxLength(500);

        builder.HasData(
            new User()
            {
                Id = 1,
                Username = "first admin",
                Bio = "I am admin of this app",
                Email = "admin123@gmail.com",
                PasswordHash = "ncE/vkagQZft0U5DxV0Z4IbHNBWgVkt/1RC/haf3nPg=", // admin123
                PasswordSalt = "oNsJmAzkVehBjvRvQta4DtP3DveFpzniZ50nST4F2Pg="
            }
        );

    }
}