using Microsoft.EntityFrameworkCore;
using social_media_console_app.Models;

namespace social_media_console_app.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Comment> Comments   { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Message> Messages   { get; set; }
    public DbSet<Post>    Posts      { get; set; }
    public DbSet<User>    Users      { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=SocialMediaDb;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}