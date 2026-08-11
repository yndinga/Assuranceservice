using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CombineTransitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Transit",
                table: "Voyages",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Voyages]
                SET [Transit] = CASE
                    WHEN NULLIF(LTRIM(RTRIM([LieuSejour])), N'') IS NOT NULL
                     AND NULLIF(LTRIM(RTRIM([DureeSejour])), N'') IS NOT NULL
                        THEN CONCAT(LTRIM(RTRIM([LieuSejour])), N' - ', LTRIM(RTRIM([DureeSejour])))
                    WHEN NULLIF(LTRIM(RTRIM([LieuSejour])), N'') IS NOT NULL
                        THEN LTRIM(RTRIM([LieuSejour]))
                    WHEN NULLIF(LTRIM(RTRIM([DureeSejour])), N'') IS NOT NULL
                        THEN LTRIM(RTRIM([DureeSejour]))
                    ELSE NULL
                END;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Transit", table: "Voyages");
        }
    }
}
