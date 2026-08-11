using AssuranceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

[DbContext(typeof(AssuranceDbContext))]
[Migration("20260625170000_RenameAerienModeAEToAR")]
public partial class RenameAerienModeAEToAR : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Assurances', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'ModeDeTransport')
            BEGIN
                EXEC(N'
                UPDATE [dbo].[Assurances]
                SET [ModeDeTransport] = N''AR''
                WHERE UPPER(LTRIM(RTRIM([ModeDeTransport]))) = N''AE'';
                ');
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModeDeTransport')
            BEGIN
                EXEC(N'
                UPDATE [dbo].[Voyages]
                SET [ModeDeTransport] = N''AR''
                WHERE UPPER(LTRIM(RTRIM([ModeDeTransport]))) = N''AE'';
                ');
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Assurances', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Assurances') AND name = N'ModeDeTransport')
            BEGIN
                EXEC(N'
                UPDATE [dbo].[Assurances]
                SET [ModeDeTransport] = N''AE''
                WHERE UPPER(LTRIM(RTRIM([ModeDeTransport]))) = N''AR'';
                ');
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Voyages', N'U') IS NOT NULL
               AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Voyages') AND name = N'ModeDeTransport')
            BEGIN
                EXEC(N'
                UPDATE [dbo].[Voyages]
                SET [ModeDeTransport] = N''AE''
                WHERE UPPER(LTRIM(RTRIM([ModeDeTransport]))) = N''AR'';
                ');
            END
            """);
    }
}
