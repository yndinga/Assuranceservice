using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssuranceStatutToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convertir la colonne Statut de nvarchar vers int (valeurs: 0=EnAttente, 1=Elaborer, 2=Created, 3=MarchandisesAdded, 4=PrimeCalculated, 5=VisaDemandé, 6=Failed, 7=Completed)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = 'Statut')
                BEGIN
                    ALTER TABLE [dbo].[Assurances] ADD [StatutInt] int NOT NULL DEFAULT 1;
                    UPDATE [dbo].[Assurances] SET [StatutInt] = CASE
                        WHEN [Statut] IN (N'Elaborer', N'elaborer') THEN 1
                        WHEN [Statut] IN (N'Created', N'created') THEN 2
                        WHEN [Statut] IN (N'MarchandisesAdded', N'marchandisesAdded') THEN 3
                        WHEN [Statut] IN (N'PrimeCalculated', N'primeCalculated') THEN 4
                        WHEN [Statut] IN (N'VisaDemandé', N'visaDemandé') THEN 5
                        WHEN [Statut] IN (N'Failed', N'failed') THEN 6
                        WHEN [Statut] IN (N'Completed', N'completed') THEN 7
                        ELSE 0
                    END;
                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [Statut];
                    EXEC sp_rename N'[dbo].[Assurances].[StatutInt]', N'Statut', N'COLUMN';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = 'Statut')
                BEGIN
                    ALTER TABLE [dbo].[Assurances] ADD [StatutNvarchar] nvarchar(250) NOT NULL DEFAULT N'EnAttente';
                    UPDATE [dbo].[Assurances] SET [StatutNvarchar] = CASE [Statut]
                        WHEN 0 THEN N'EnAttente'
                        WHEN 1 THEN N'Elaborer'
                        WHEN 2 THEN N'Created'
                        WHEN 3 THEN N'MarchandisesAdded'
                        WHEN 4 THEN N'PrimeCalculated'
                        WHEN 5 THEN N'VisaDemandé'
                        WHEN 6 THEN N'Failed'
                        WHEN 7 THEN N'Completed'
                        ELSE N'EnAttente'
                    END;
                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [Statut];
                    EXEC sp_rename N'[dbo].[Assurances].[StatutNvarchar]', N'Statut', N'COLUMN';
                END
            ");
        }
    }
}
