using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.ProjectConstants;
using social_media_console_app.Models;

namespace social_media_console_app.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table => table.HasCheckConstraint("CK_User_DateOfBirth", 
            "DATEDIFF(year, DateOfBirth, GETUTCDATE()) BETWEEN 13 AND 100") );
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Username)
            .IsRequired()
            .HasMaxLength(Constants.UsernameMaxlength);
        
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(Constants.EmailMaxLength);

        builder.Property(user => user.DateOfBirth)
            .IsRequired();

        builder.HasIndex(user => user.Username).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .IsFixedLength()
            .HasMaxLength(Constants.PasswordHashMaxLength);
        
        builder.Property(user => user.PasswordSalt)
            .IsRequired()
            .IsFixedLength()
            .HasMaxLength(Constants.PasswordSaltMaxLength);

        builder.Property(user => user.Bio)
            .HasMaxLength(Constants.BioMaxLength);

        builder.HasData(
            new User()
            {
                Id = 1,
                Username = "first_admin",
                Bio = "I am admin of this app",
                Email = "admin123@gmail.com",
                PasswordHash = "ncE/vkagQZft0U5DxV0Z4IbHNBWgVkt/1RC/haf3nPg=", // admin123
                PasswordSalt = "oNsJmAzkVehBjvRvQta4DtP3DveFpzniZ50nST4F2Pg=",
                DateOfBirth = new DateTime(2000, 4, 17, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 5, 14, 18, 29, 30, DateTimeKind.Utc)
            },
            new User
            {
                Id = 2,
                Username = "second_admin",
                Bio = "I am the second admin of this app",
                Email = "secondadmin123@gmail.com",
                PasswordHash = "ncE/vkagQZft0U5DxV0Z4IbHNBWgVkt/1RC/haf3nPg=", // admin123
                PasswordSalt = "oNsJmAzkVehBjvRvQta4DtP3DveFpzniZ50nST4F2Pg=",
                DateOfBirth = new DateTime(2005, 12, 17, 0,  0,  0,  DateTimeKind.Utc),
                CreatedAt = new DateTime(2026,   5, 14, 18, 40, 30, DateTimeKind.Utc)
            }
        );

    }
}