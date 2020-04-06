using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodingEventsAPI.Data.Migrations
{
    public partial class CodingEventTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodingEventTag",
                columns: table => new
                {
                    TagId = table.Column<long>(nullable: false),
                    CodingEventId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodingEventTag", x => new { x.TagId, x.CodingEventId });
                    table.ForeignKey(
                        name: "FK_CodingEventTag_CodingEvents_CodingEventId",
                        column: x => x.CodingEventId,
                        principalTable: "CodingEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodingEventTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodingEventTag_CodingEventId",
                table: "CodingEventTag",
                column: "CodingEventId");

            migrationBuilder.CreateIndex(
                name: "IX_CodingEventTag_TagId_CodingEventId",
                table: "CodingEventTag",
                columns: new[] { "TagId", "CodingEventId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodingEventTag");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
