using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatService.Migrations
{
    /// <inheritdoc />
    public partial class ConversationPerParticipantPair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_BookingId",
                table: "Conversations");

            migrationBuilder.Sql("""
                DELETE FROM "Conversations" AS duplicate
                USING "Conversations" AS keeper
                WHERE duplicate."PassengerId" = keeper."PassengerId"
                  AND duplicate."DriverId" = keeper."DriverId"
                  AND duplicate."ConversationId"::text > keeper."ConversationId"::text;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_PassengerId_DriverId",
                table: "Conversations",
                columns: new[] { "PassengerId", "DriverId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conversations_PassengerId_DriverId",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_BookingId",
                table: "Conversations",
                column: "BookingId",
                unique: true);
        }
    }
}
