using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.IO;

namespace MyPlanner.Models
{
    public class PlanerDbContextFactory : IDesignTimeDbContextFactory<PlanerDbContext>
    {
        public PlanerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlanerDbContext>();

            // Получаем строку подключения из файла конфигурации
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseLazyLoadingProxies();

            return new PlanerDbContext(optionsBuilder.Options);
        }
    }
}
