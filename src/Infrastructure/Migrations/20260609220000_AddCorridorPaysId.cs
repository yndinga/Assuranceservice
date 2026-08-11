using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260609220000_AddCorridorPaysId")]
public partial class AddCorridorPaysId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Corridors', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Corridors') AND name = N'PaysId')
                ALTER TABLE [Corridors] ADD [PaysId] uniqueidentifier NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Corridors') AND name = N'PaysId')
                ALTER TABLE [Corridors] DROP COLUMN [PaysId];
            """);
    }
}
