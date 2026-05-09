using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Models;

namespace social_media_console_app.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.CommentContent)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasOne(comment => comment.CommenterUser)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.CommenterUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(comment => comment.Post)
            .WithMany(post => post.Comments)
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}