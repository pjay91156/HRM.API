using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFloorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The [hrm].[floor] table, its PK/UQ constraints (pk_floor, uq_floor_name) and
            // FKs to [hrm].[users] (fk_floor_created_by, fk_floor_updated_by) were already
            // created manually against the database. This migration only exists to keep the
            // EF model snapshot in sync with the Floor entity, so Up() is intentionally a no-op.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally a no-op — see Up().
        }
    }
}
