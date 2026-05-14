using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Models;

namespace social_media_console_app.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.CommentContent)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(comment => comment.CommenterUser)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.CommenterUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(comment => comment.Post)
            .WithMany(post => post.Comments)
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasData(
            new Comment
            {
                Id = 1,
                CommenterUserId = 2,
                PostId = 1,
                CommentContent = "Nice post!",
                CreatedAt = new DateTime(2026, 5, 14, 18, 42, 34, DateTimeKind.Utc),
            }
        );
    }
}