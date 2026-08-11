using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssuranceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTronconCorridorRouteIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Corridors_CorridorId')
                    ALTER TABLE [Troncons] DROP CONSTRAINT [FK_Troncons_Corridors_CorridorId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Troncons_Routes_RouteId')
                    ALTER TABLE [Troncons] DROP CONSTRAINT [FK_Troncons_Routes_RouteId];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_CorridorId' AND object_id = OBJECT_ID(N'dbo.Troncons'))
                    DROP INDEX [IX_Troncons_CorridorId] ON [Troncons];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Troncons_RouteId' AND object_id = OBJECT_ID(N'dbo.Troncons'))
                    DROP INDEX [IX_Troncons_RouteId] ON [Troncons];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Troncons') AND name = N'CorridorId')
                    ALTER TABLE [Troncons] DROP COLUMN [CorridorId];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Troncons') AND name = N'RouteId')
                    ALTER TABLE [Troncons] DROP COLUMN [RouteId];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CorridorId",
                table: "Troncons",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RouteId",
                table: "Troncons",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Troncons_CorridorId",
                table: "Troncons",
                column: "CorridorId");

            migrationBuilder.CreateIndex(
                name: "IX_Troncons_RouteId",
                table: "Troncons",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Troncons_Corridors_CorridorId",
                table: "Troncons",
                column: "CorridorId",
                principalTable: "Corridors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Troncons_Routes_RouteId",
                table: "Troncons",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
