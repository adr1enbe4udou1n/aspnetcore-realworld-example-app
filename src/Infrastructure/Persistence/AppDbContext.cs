using System;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Slugify;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        private readonly ISlugHelper _slugHelper;
        private readonly ICurrentUser _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ISlugHelper slugHelper, ICurrentUser currentUser) : base(options)
        {
            _slugHelper = slugHelper;
            _currentUser = currentUser;

            ChangeTracker.StateChanged += UpdateTimestamps;
            ChangeTracker.StateChanged += UpdateSlug;
            ChangeTracker.StateChanged += UpdateAuthor;
            ChangeTracker.Tracked += UpdateTimestamps;
            ChangeTracker.Tracked += UpdateSlug;
            ChangeTracker.Tracked += UpdateAuthor;
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

        private void UpdateSlug(object sender, EntityEntryEventArgs e)
        {
            if (e.Entry.Entity is IHasSlug entity)
            {
                if (entity.Slug == null)
                {
                    entity.Slug = _slugHelper.GenerateSlug(entity.GetSlugSource());
                }
            }
        }

        private void UpdateAuthor(object sender, EntityEntryEventArgs e)
        {
            if (e.Entry.Entity is IHasAuthor entity)
            {
                if (entity.Author == null)
                {
                    entity.AuthorId = _currentUser.User.Id;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique()
            ;

            builder.Entity<Article>(b =>
            {
                b.HasIndex(u => u.Slug).IsUnique();

                b.HasOne(e => e.Author)
                    .WithMany(e => e.Articles)
                    .HasForeignKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade)
                ;
            });

            builder.Entity<ArticleTag>(b =>
            {
                b.HasKey(e => new { e.ArticleId, e.TagId });

                b.HasOne(e => e.Article)
                    .WithMany(e => e.Tags)
                    .HasForeignKey(e => e.ArticleId);

                b.HasOne(e => e.Tag)
                    .WithMany(e => e.Articles)
                    .HasForeignKey(e => e.TagId);
            });

            builder.Entity<ArticleFavorite>(b =>
            {
                b.HasKey(e => new { e.ArticleId, e.UserId });

                b.HasOne(e => e.Article)
                    .WithMany(e => e.FavoredUsers)
                    .HasForeignKey(e => e.ArticleId);

                b.HasOne(e => e.User)
                    .WithMany(e => e.FavoriteArticles)
                    .HasForeignKey(e => e.UserId);
            });

            builder.Entity<FollowerUser>(b =>
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