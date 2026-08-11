using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

public partial class RenameAvenantToAvenant : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Avenants]', N'U') IS NOT NULL AND OBJECT_ID(N'[dbo].[Avenants]', N'U') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Avenants', N'Avenants';
            END

            IF COL_LENGTH(N'dbo.Avenants', N'NoAvenant') IS NOT NULL AND COL_LENGTH(N'dbo.Avenants', N'NoAvenant') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Avenants.NoAvenant', N'NoAvenant', N'COLUMN';
            END

            IF COL_LENGTH(N'dbo.Historiques', N'AvenantId') IS NOT NULL AND COL_LENGTH(N'dbo.Historiques', N'AvenantId') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Historiques.AvenantId', N'AvenantId', N'COLUMN';
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'dbo.Historiques', N'AvenantId') IS NOT NULL AND COL_LENGTH(N'dbo.Historiques', N'AvenantId') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Historiques.AvenantId', N'AvenantId', N'COLUMN';
            END

            IF COL_LENGTH(N'dbo.Avenants', N'NoAvenant') IS NOT NULL AND COL_LENGTH(N'dbo.Avenants', N'NoAvenant') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Avenants.NoAvenant', N'NoAvenant', N'COLUMN';
            END

            IF OBJECT_ID(N'[dbo].[Avenants]', N'U') IS NOT NULL AND OBJECT_ID(N'[dbo].[Avenants]', N'U') IS NULL
            BEGIN
                EXEC sp_rename N'dbo.Avenants', N'Avenants';
            END
            """);
    }
}
