using Microsoft.EntityFrameworkCore;

namespace CursovaRobota
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages { get; set; }
         public DbSet<FavoriteItem> Favorites { get; set; } 
    }
}