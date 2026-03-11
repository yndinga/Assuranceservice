using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropPoliciesAndCustomersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables template Policy/Customer non utilisées par le métier (Assurance) — suppression.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[Policies]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[Policies];
                IF OBJECT_ID(N'[dbo].[Customers]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[Customers];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recréation non prévue : ces tables ne font pas partie du modèle métier.
        }
    }
}
