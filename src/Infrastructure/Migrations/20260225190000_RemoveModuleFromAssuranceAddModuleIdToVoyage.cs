using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModuleFromAssuranceAddModuleIdToVoyage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Retirer Module de la table Assurances
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]')
                    AND name = 'Module'
                )
                BEGIN
                    ALTER TABLE [dbo].[Assurances] DROP COLUMN [Module];
                END
            ");

            // Ajouter ModuleId à la table Voyages (FK vers Modules)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]')
                    AND name = 'ModuleId'
                )
                BEGIN
                    ALTER TABLE [dbo].[Voyages]
                    ADD [ModuleId] uniqueidentifier NULL;

                    CREATE INDEX [IX_Voyages_ModuleId] ON [dbo].[Voyages] ([ModuleId]);

                    ALTER TABLE [dbo].[Voyages]
                    ADD CONSTRAINT [FK_Voyages_Modules_ModuleId]
                    FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Modules] ([Id])
                    ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Retirer ModuleId de Voyages
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]') AND name = 'ModuleId')
                BEGIN
                    ALTER TABLE [dbo].[Voyages] DROP CONSTRAINT [FK_Voyages_Modules_ModuleId];
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Voyages_ModuleId' AND object_id = OBJECT_ID(N'[dbo].[Voyages]'))
                        DROP INDEX [IX_Voyages_ModuleId] ON [dbo].[Voyages];
                    ALTER TABLE [dbo].[Voyages] DROP COLUMN [ModuleId];
                END
            ");

            // Remettre Module dans Assurances
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assurances]') AND name = 'Module')
                BEGIN
                    ALTER TABLE [dbo].[Assurances] ADD [Module] nvarchar(25) NOT NULL DEFAULT '';
                END
            ");
        }
    }
}
