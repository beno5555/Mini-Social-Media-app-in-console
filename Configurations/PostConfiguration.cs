using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Identity.Client.RP;
using social_media_console_app.Models;

namespace social_media_console_app.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(post => post.Id);

        builder.Property(post => post.PostTitle)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(post => post.PostContent)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(post => post.User)
            .WithMany(user => user.Posts)
            .HasForeignKey(post => post.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new Post
            {
                Id = 1,
                UserId = 1,
                PostTitle = "Initial admin post",
                PostContent = "this is initial admin post"
            }
        );
    }
}