using Conduit.Application.Interfaces;
using Conduit.Domain.Entities;
using Conduit.Infrastructure.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Conduit.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options), IAppDbContext
{
    private static readonly AuditableInterceptor AuditableInterceptor = new();

    public DbSet<User> Users => Set<User>();
    public DbSet<FollowerUser> FollowerUser => Set<FollowerUser>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleFavorite> ArticleFavorite => Set<ArticleFavorite>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(AuditableInterceptor);
    }

    public void UseRoConnection()
    {
        var roConnectionString = configuration.GetConnectionString("DefaultRoConnection");

        if (roConnectionString != null)
        {
            Database.SetConnectionString(roConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique()
        ;

        modelBuilder.Entity<Tag>()
            .HasIndex(u => u.Name).IsUnique()
        ;

        modelBuilder.Entity<Article>(b =>
        {
            b.HasIndex(u => u.Slug).IsUnique();

            b.HasOne(e => e.Author)
                .WithMany(e => e.Articles)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Cascade)
            ;
        });

        modelBuilder.Entity<ArticleTag>(b =>
        {
            b.HasKey(e => new { e.ArticleId, e.TagId });

            b.HasOne(e => e.Article)
                .WithMany(e => e.Tags)
                .HasForeignKey(e => e.ArticleId);

            b.HasOne(e => e.Tag)
                .WithMany(e => e.Articles)
                .HasForeignKey(e => e.TagId);
        });

        modelBuilder.Entity<ArticleFavorite>(b =>
        {
            b.HasKey(e => new { e.ArticleId, e.UserId });

            b.HasOne(e => e.Article)
                .WithMany(e => e.FavoredUsers)
                .HasForeignKey(e => e.ArticleId);

            b.HasOne(e => e.User)
                .WithMany(e => e.FavoriteArticles)
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<FollowerUser>(b =>
        {
            b.HasKey(e => new { e.FollowingId, e.FollowerId });

            b.HasOne(e => e.Following)
                .WithMany(e => e.Followers)
                .HasForeignKey(e => e.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(e => e.Follower)
                .WithMany(e => e.Following)
                .HasForeignKey(e => e.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}