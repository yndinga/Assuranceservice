using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AssuranceService.Infrastructure.Data;

public class AssuranceDbContextFactory : IDesignTimeDbContextFactory<AssuranceDbContext>
{
    public AssuranceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AssuranceDbContext>();
        
        // Connection string pour les migrations (mode design-time)
        var connectionString = "Server=localhost, 1420;Database=MS_ASSURANCE;User Id=sa;Password=DevStrongPwd@123;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new AssuranceDbContext(optionsBuilder.Options);
    }
}
