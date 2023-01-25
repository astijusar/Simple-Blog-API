using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SimpleBlogAPI.Models
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)
        { 

        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .Property(c => c.CreatedOn)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Category>()
                .Property(c => c.CreatedOn)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Post>()
                .Property(p => p.CreatedOn)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Post>()
                .Property(p => p.LastModifiedOn)
                .HasDefaultValueSql("NOW()")
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}
