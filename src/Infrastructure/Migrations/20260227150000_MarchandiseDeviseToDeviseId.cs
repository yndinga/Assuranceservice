using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarchandiseDeviseToDeviseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Marchandises]') AND name = 'DeviseId')
                BEGIN
                    ALTER TABLE [dbo].[Marchandises] ADD [DeviseId] uniqueidentifier NULL;

                    UPDATE m
                    SET m.DeviseId = d.Id
                    FROM [dbo].[Marchandises] m
                    INNER JOIN [dbo].[Devises] d ON LTRIM(RTRIM(d.Code)) = LTRIM(RTRIM(m.Devise));

                    UPDATE m
                    SET m.DeviseId = (SELECT TOP 1 Id FROM [dbo].[Devises] WHERE LTRIM(RTRIM(Code)) = N'XOF')
                    FROM [dbo].[Marchandises] m
                    WHERE m.DeviseId IS NULL;

                    ALTER TABLE [dbo].[Marchandises] ALTER COLUMN [DeviseId] uniqueidentifier NOT NULL;

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Marchandises]') AND name = 'Devise')
                        ALTER TABLE [dbo].[Marchandises] DROP COLUMN [Devise];

                    CREATE INDEX [IX_Marchandises_DeviseId] ON [dbo].[Marchandises] ([DeviseId]);

                    ALTER TABLE [dbo].[Marchandises]
                    ADD CONSTRAINT [FK_Marchandises_Devises_DeviseId]
                    FOREIGN KEY ([DeviseId]) REFERENCES [dbo].[Devises] ([Id])
                    ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Marchandises]') AND name = 'DeviseId')
                BEGIN
                    ALTER TABLE [dbo].[Marchandises] DROP CONSTRAINT [FK_Marchandises_Devises_DeviseId];
                    DROP INDEX IF EXISTS [IX_Marchandises_DeviseId] ON [dbo].[Marchandises];

                    ALTER TABLE [dbo].[Marchandises] ADD [Devise] nvarchar(50) NOT NULL DEFAULT N'XOF';

                    UPDATE m
                    SET m.Devise = d.Code
                    FROM [dbo].[Marchandises] m
                    INNER JOIN [dbo].[Devises] d ON d.Id = m.DeviseId;

                    ALTER TABLE [dbo].[Marchandises] DROP COLUMN [DeviseId];
                END
            ");
        }
    }
}
