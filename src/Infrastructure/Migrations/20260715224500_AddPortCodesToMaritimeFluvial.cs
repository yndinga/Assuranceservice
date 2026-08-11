using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260715224500_AddPortCodesToMaritimeFluvial")]
public partial class AddPortCodesToMaritimeFluvial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Maritimes]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'[dbo].[Maritimes]', N'PortEmbarquementCode') IS NULL
                    ALTER TABLE [dbo].[Maritimes] ADD [PortEmbarquementCode] nvarchar(50) NULL;

                IF COL_LENGTH(N'[dbo].[Maritimes]', N'PortDebarquementCode') IS NULL
                    ALTER TABLE [dbo].[Maritimes] ADD [PortDebarquementCode] nvarchar(50) NULL;

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Maritimes_Ports_PortEmbarquementId')
                    ALTER TABLE [dbo].[Maritimes] DROP CONSTRAINT [FK_Maritimes_Ports_PortEmbarquementId];

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Maritimes_Ports_PortDebarquementId')
                    ALTER TABLE [dbo].[Maritimes] DROP CONSTRAINT [FK_Maritimes_Ports_PortDebarquementId];

                IF COL_LENGTH(N'[dbo].[Maritimes]', N'PortEmbarquementId') IS NOT NULL
                    ALTER TABLE [dbo].[Maritimes] ALTER COLUMN [PortEmbarquementId] uniqueidentifier NULL;

                IF COL_LENGTH(N'[dbo].[Maritimes]', N'PortDebarquementId') IS NOT NULL
                    ALTER TABLE [dbo].[Maritimes] ALTER COLUMN [PortDebarquementId] uniqueidentifier NULL;
            END

            IF OBJECT_ID(N'[dbo].[Fluviaux]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'[dbo].[Fluviaux]', N'PortEmbarquementCode') IS NULL
                    ALTER TABLE [dbo].[Fluviaux] ADD [PortEmbarquementCode] nvarchar(50) NULL;

                IF COL_LENGTH(N'[dbo].[Fluviaux]', N'PortDebarquementCode') IS NULL
                    ALTER TABLE [dbo].[Fluviaux] ADD [PortDebarquementCode] nvarchar(50) NULL;

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Fluviaux_Ports_PortEmbarquementId')
                    ALTER TABLE [dbo].[Fluviaux] DROP CONSTRAINT [FK_Fluviaux_Ports_PortEmbarquementId];

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Fluviaux_Ports_PortDebarquementId')
                    ALTER TABLE [dbo].[Fluviaux] DROP CONSTRAINT [FK_Fluviaux_Ports_PortDebarquementId];

                IF COL_LENGTH(N'[dbo].[Fluviaux]', N'PortEmbarquementId') IS NOT NULL
                    ALTER TABLE [dbo].[Fluviaux] ALTER COLUMN [PortEmbarquementId] uniqueidentifier NULL;

                IF COL_LENGTH(N'[dbo].[Fluviaux]', N'PortDebarquementId') IS NOT NULL
                    ALTER TABLE [dbo].[Fluviaux] ALTER COLUMN [PortDebarquementId] uniqueidentifier NULL;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Maritimes]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'[dbo].[Maritimes]', N'PortDebarquementCode') IS NOT NULL
                    ALTER TABLE [dbo].[Maritimes] DROP COLUMN [PortDebarquementCode];

                IF COL_LENGTH(N'[dbo].[Maritimes]', N'PortEmbarquementCode') IS NOT NULL
                    ALTER TABLE [dbo].[Maritimes] DROP COLUMN [PortEmbarquementCode];
            END

            IF OBJECT_ID(N'[dbo].[Fluviaux]', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'[dbo].[Fluviaux]', N'PortDebarquementCode') IS NOT NULL
                    ALTER TABLE [dbo].[Fluviaux] DROP COLUMN [PortDebarquementCode];

                IF COL_LENGTH(N'[dbo].[Fluviaux]', N'PortEmbarquementCode') IS NOT NULL
                    ALTER TABLE [dbo].[Fluviaux] DROP COLUMN [PortEmbarquementCode];
            END
            """);
    }
}
