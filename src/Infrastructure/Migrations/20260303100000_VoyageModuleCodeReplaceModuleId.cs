using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VoyageModuleCodeReplaceModuleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Supprimer ModuleId (FK, index, colonne) si présents
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]') AND name = 'ModuleId')
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Voyages_Modules_ModuleId')
                        ALTER TABLE [dbo].[Voyages] DROP CONSTRAINT [FK_Voyages_Modules_ModuleId];
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Voyages_ModuleId' AND object_id = OBJECT_ID(N'[dbo].[Voyages]'))
                        DROP INDEX [IX_Voyages_ModuleId] ON [dbo].[Voyages];
                    ALTER TABLE [dbo].[Voyages] DROP COLUMN [ModuleId];
                END
            ");

            // 2. Ajouter ModuleCode (obligatoire pour l'application)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]') AND name = 'ModuleCode')
                BEGIN
                    ALTER TABLE [dbo].[Voyages] ADD [ModuleCode] nvarchar(10) NOT NULL DEFAULT 'MA';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]') AND name = 'ModuleCode')
                    ALTER TABLE [dbo].[Voyages] DROP COLUMN [ModuleCode];
            ");
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Voyages]') AND name = 'ModuleId')
                BEGIN
                    ALTER TABLE [dbo].[Voyages] ADD [ModuleId] uniqueidentifier NULL;
                    CREATE INDEX [IX_Voyages_ModuleId] ON [dbo].[Voyages] ([ModuleId]);
                END
            ");
        }
    }
}
