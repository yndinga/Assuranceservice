using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEtats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Etats', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Etats] (
                        [Id] uniqueidentifier NOT NULL,
                        [Libelle] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [CodeEcran] nvarchar(450) NOT NULL,
                        [Actif] bit NOT NULL,
                        [CreerPar] nvarchar(max) NULL,
                        [ModifierPar] nvarchar(max) NULL,
                        [CreerLe] datetime2 NOT NULL,
                        [ModifierLe] datetime2 NULL,
                        CONSTRAINT [PK_Etats] PRIMARY KEY ([Id])
                    );
                    CREATE UNIQUE INDEX [IX_Etats_CodeEcran] ON [Etats] ([CodeEcran]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Etats', N'U') IS NOT NULL
                    DROP TABLE [Etats];
                """);
        }
    }
}
