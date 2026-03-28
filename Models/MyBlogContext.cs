using Microsoft.EntityFrameworkCore;

namespace CS58.Models;
// CS58.Models.MyBlogContext
public class MyBlogContext : DbContext
{
    public MyBlogContext(DbContextOptions<MyBlogContext> options) : base(options)
    {
    }

    protected MyBlogContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Article> articles {get; set;}
}