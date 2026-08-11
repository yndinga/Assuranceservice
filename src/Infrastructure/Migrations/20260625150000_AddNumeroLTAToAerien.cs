using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260625150000_AddNumeroLTAToAerien")]
public partial class AddNumeroLTAToAerien : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Aeriens', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Aeriens') AND name = N'NumeroLTA')
                ALTER TABLE [dbo].[Aeriens] ADD [NumeroLTA] nvarchar(255) NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Aeriens', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Aeriens') AND name = N'NumeroLTA')
                ALTER TABLE [dbo].[Aeriens] DROP COLUMN [NumeroLTA];
            """);
    }
}
