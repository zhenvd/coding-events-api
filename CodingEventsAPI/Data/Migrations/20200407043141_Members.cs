using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodingEventsAPI.Data.Migrations
{
    public partial class Members : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodingEventTag_CodingEvents_CodingEventId",
                table: "CodingEventTag");

            migrationBuilder.DropForeignKey(
                name: "FK_CodingEventTag_Tags_TagId",
                table: "CodingEventTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodingEventTag",
                table: "CodingEventTag");

            migrationBuilder.DropIndex(
                name: "IX_CodingEventTag_CodingEventId",
                table: "CodingEventTag");

            migrationBuilder.RenameTable(
                name: "CodingEventTag",
                newName: "CodingEventTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodingEventTags",
                table: "CodingEventTags",
                columns: new[] { "CodingEventId", "TagId" });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AzureOId = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Role = table.Column<int>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    CodingEventId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_CodingEvents_CodingEventId",
                        column: x => x.CodingEventId,
                        principalTable: "CodingEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Members_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodingEventTags_TagId",
                table: "CodingEventTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CodingEventId",
                table: "Members",
                column: "CodingEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId_CodingEventId",
                table: "Members",
                columns: new[] { "UserId", "CodingEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AzureOId",
                table: "Users",
                column: "AzureOId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CodingEventTags_CodingEvents_CodingEventId",
                table: "CodingEventTags",
                column: "CodingEventId",
                principalTable: "CodingEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CodingEventTags_Tags_TagId",
                table: "CodingEventTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodingEventTags_CodingEvents_CodingEventId",
                table: "CodingEventTags");

            migrationBuilder.DropForeignKey(
                name: "FK_CodingEventTags_Tags_TagId",
                table: "CodingEventTags");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodingEventTags",
                table: "CodingEventTags");

            migrationBuilder.DropIndex(
                name: "IX_CodingEventTags_TagId",
                table: "CodingEventTags");

            migrationBuilder.RenameTable(
                name: "CodingEventTags",
                newName: "CodingEventTag");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodingEventTag",
                table: "CodingEventTag",
                columns: new[] { "TagId", "CodingEventId" });

            migrationBuilder.CreateIndex(
                name: "IX_CodingEventTag_CodingEventId",
                table: "CodingEventTag",
                column: "CodingEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CodingEventTag_CodingEvents_CodingEventId",
                table: "CodingEventTag",
                column: "CodingEventId",
                principalTable: "CodingEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CodingEventTag_Tags_TagId",
                table: "CodingEventTag",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
