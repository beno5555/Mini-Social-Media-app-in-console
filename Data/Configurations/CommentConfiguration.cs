using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using social_media_console_app.Models;
using social_media_console_app.ProjectConstants;

namespace social_media_console_app.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.CommentContent)
            .IsRequired()
            .HasMaxLength(Constants.CommentMaxLength);

        builder.HasOne(comment => comment.CommenterUser)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.CommenterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(comment => comment.Post)
            .WithMany(post => post.Comments)
            .HasForeignKey(comment => comment.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}