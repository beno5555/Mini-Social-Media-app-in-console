using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Models;
using social_media_console_app.ProjectConstants.Enums;

namespace social_media_console_app.Data.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");
        builder.HasKey(friendship => new { friendship.RequesterUserId, friendship.AddresseeUserId });

        builder.Property(friendship => friendship.FriendshipStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();
        
        builder.HasOne(friendship => friendship.RequesterUser)
            .WithMany(requesterUser => requesterUser.SentFriendRequests)
            .HasForeignKey(friendship => friendship.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(friendship => friendship.AddresseeUser)
            .WithMany(addresseeUser => addresseeUser.ReceivedFriendRequests)
            .HasForeignKey(friendship => friendship.AddresseeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Friendship
            {
                AddresseeUserId = 1,
                RequesterUserId = 2,
                CreatedAt = new DateTime(2026, 5, 14, 18, 43, 58, DateTimeKind.Utc),
                FriendshipStatus = FriendshipStatus.Accepted
            });
    }
}