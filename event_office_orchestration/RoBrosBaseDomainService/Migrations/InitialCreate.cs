using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoBrosBaseDomainService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entity_journals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    entity = table.Column<string>(type: "jsonb", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_journals", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_entity_journals_created_at",
                table: "entity_journals",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_entity_journals_entity_lookup",
                table: "entity_journals",
                columns: new[] { "entity_id", "entity_type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_entity_journals_entity_version",
                table: "entity_journals",
                columns: new[] { "entity_id", "version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_journals");
        }
    }
}