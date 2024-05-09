using Microsoft.EntityFrameworkCore;
namespace Bloging;
public class BlogingDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    public BlogingDbContext(DbContextOptions<BlogingDbContext> options):base(options)
    {
        Database.EnsureCreated();
    }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");
}

public class Blog
{
    public string CompilerServices { get; set; }        
    public string BlogId { get; set; }
    public string Url { get; set; }

    public List<Post> Posts { get; set; }
}

public class Post
{
    public string PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public string BlogId { get; set; }
    public Blog Blog { get; set; }
}