using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260626120000_CleanupVoyageModuleId")]
public partial class CleanupVoyageModuleId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModuleId')
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Voyages_Modules_ModuleId')
                    ALTER TABLE [dbo].[Voyages] DROP CONSTRAINT [FK_Voyages_Modules_ModuleId];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'IX_Voyages_ModuleId')
                    DROP INDEX [IX_Voyages_ModuleId] ON [dbo].[Voyages];

                ALTER TABLE [dbo].[Voyages] DROP COLUMN [ModuleId];
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModuleId')
                ALTER TABLE [dbo].[Voyages] ADD [ModuleId] uniqueidentifier NULL;
            """);
    }
}
