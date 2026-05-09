using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Models;

namespace social_media_console_app.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.MessageContent)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(message => message.SenderUser)
            .WithMany(senderUser => senderUser.SentMessages)
            .HasForeignKey(message => message.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(message => message.ReceiverUser)
            .WithMany(receiverUser => receiverUser.ReceivedMessages)
            .HasForeignKey(message => message.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}