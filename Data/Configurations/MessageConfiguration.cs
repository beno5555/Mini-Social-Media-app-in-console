using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Constants;
using social_media_console_app.Models;

namespace social_media_console_app.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.MessageContent)
            .IsRequired()
            .HasMaxLength(Constraints.MessageMaxLength);

        // service implementation must manually delete the user's messages before deleting the user.
        builder.HasOne(message => message.SenderUser)
            .WithMany(senderUser => senderUser.SentMessages)
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(message => message.ReceiverUser)
            .WithMany(receiverUser => receiverUser.ReceivedMessages)
            .HasForeignKey(message => message.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Message
            {
                Id = 1,
                SenderUserId = 2, 
                ReceiverUserId = 1,
                MessageContent = "Hello first admin! i am the second admin",
                IsRead = false,
                CreatedAt = new DateTime(2026, 5, 14, 18, 47, 58, DateTimeKind.Utc),
                
            });
    }
}