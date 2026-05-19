using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CursovaRobota
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // ВСТАВ СЮДИ СВІЙ РЕАЛЬНИЙ Connection String з Railway
            // Він виглядає як: Host=...;Port=...;Database=...;Username=...;Password=...
           string connectionString = "Host=yamabiko.proxy.rlwy.net;Port=57448;Database=railway;Username=postgres;Password=wQLJtJkZGTETvVRTcwadlhDWwFWJuJgH";
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}