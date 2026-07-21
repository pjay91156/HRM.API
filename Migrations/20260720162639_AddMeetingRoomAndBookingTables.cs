using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetingRoomAndBookingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The [hrm].[meeting_room], [hrm].[meeting_room_amenity],
            // [hrm].[meeting_room_amenity_detail] and [hrm].[meeting_room_booking] tables,
            // along with their PK/UQ/CHECK constraints and FKs to [hrm].[floor]/[hrm].[users]/
            // [hrm].[employee]/[hrm].[meeting_room]/[hrm].[meeting_room_amenity], were already
            // created manually against the database. This migration only exists to keep the
            // EF model snapshot in sync with the entities, so Up() is intentionally a no-op.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally a no-op — see Up().
        }
    }
}
