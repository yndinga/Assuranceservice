using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateTypePartenaires : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.TypePartenaires', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[TypePartenaires] (
                    [Id] uniqueidentifier NOT NULL,
                    [Code] nvarchar(50) NOT NULL,
                    [Libelle] nvarchar(150) NOT NULL,
                    [Description] nvarchar(500) NULL,
                    [Actif] bit NOT NULL,
                    [CreerPar] nvarchar(max) NULL,
                    [ModifierPar] nvarchar(max) NULL,
                    [CreerLe] datetime2 NOT NULL,
                    [ModifierLe] datetime2 NULL,
                    CONSTRAINT [PK_TypePartenaires] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_TypePartenaires_Code] ON [dbo].[TypePartenaires] ([Code]);
            END

            IF NOT EXISTS (SELECT 1 FROM [dbo].[TypePartenaires] WHERE [Code] = N'ASSUREUR')
                INSERT INTO [dbo].[TypePartenaires] ([Id], [Code], [Libelle], [Description], [Actif], [CreerPar], [ModifierPar], [CreerLe], [ModifierLe])
                VALUES ('00000000-0000-0000-0000-000000000101', N'ASSUREUR', N'Assureur', N'Maison d''assurance qui porte et facture le contrat.', 1, N'Migration', N'Migration', SYSUTCDATETIME(), NULL);

            IF NOT EXISTS (SELECT 1 FROM [dbo].[TypePartenaires] WHERE [Code] = N'COURTIER')
                INSERT INTO [dbo].[TypePartenaires] ([Id], [Code], [Libelle], [Description], [Actif], [CreerPar], [ModifierPar], [CreerLe], [ModifierLe])
                VALUES ('00000000-0000-0000-0000-000000000102', N'COURTIER', N'Courtier', N'Intermediaire qui peut recevoir la demande et choisir un assureur.', 1, N'Migration', N'Migration', SYSUTCDATETIME(), NULL);

            IF NOT EXISTS (SELECT 1 FROM [dbo].[TypePartenaires] WHERE [Code] = N'AGENT_GENERAL')
                INSERT INTO [dbo].[TypePartenaires] ([Id], [Code], [Libelle], [Description], [Actif], [CreerPar], [ModifierPar], [CreerLe], [ModifierLe])
                VALUES ('00000000-0000-0000-0000-000000000103', N'AGENT_GENERAL', N'Agent general', N'Intermediaire rattache a une maison d''assurance.', 1, N'Migration', N'Migration', SYSUTCDATETIME(), NULL);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.TypePartenaires', N'U') IS NOT NULL
                DROP TABLE [dbo].[TypePartenaires];
            """);
    }
}
