using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PROJECT_PRN232_.Data
{
    // Design-time factory so `dotnet ef` can create AppDbContext without running the app
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use the same connection string you set in appsettings.json for the KENVOID server
            var connectionString = "Server=KENVOID;Database=EduBridgeDb;User Id=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
