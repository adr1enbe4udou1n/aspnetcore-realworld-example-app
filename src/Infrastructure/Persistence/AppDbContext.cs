using System;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            ChangeTracker.StateChanged += UpdateTimestamps;
            ChangeTracker.Tracked += UpdateTimestamps;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }

        private void UpdateTimestamps(object sender, EntityEntryEventArgs e)
        {
            if (e.Entry.Entity is IHasTimestamps entity)
            {
                switch (e.Entry.State)
                {
                    case EntityState.Added:
                        if (entity.CreatedAt == default)
                        {
                            entity.CreatedAt = DateTime.UtcNow;
                        }
                        entity.UpdatedAt = entity.CreatedAt;
                        break;
                    case EntityState.Modified:
                        if (entity.UpdatedAt == default)
                        {
                            entity.UpdatedAt = DateTime.UtcNow;
                        }
                        break;
                }
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
}
