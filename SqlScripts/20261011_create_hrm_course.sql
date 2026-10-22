/*
    Course Management - creates hrm.course

    Run in SSMS against the HRM database (HumanResourceManagement).
    Safe to run more than once.

    This creates exactly what the EF Core migration 20261011052123_AddCourseTable
    creates. Use either this script or `dotnet ef database update`, not both by hand:
    when the EF migration history table exists, the script records the migration
    so a later `dotnet ef database update` skips it.
*/

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF SCHEMA_ID(N'hrm') IS NULL
    EXEC(N'CREATE SCHEMA [hrm]');

IF OBJECT_ID(N'[hrm].[course]', N'U') IS NULL
BEGIN
    CREATE TABLE [hrm].[course]
    (
        [id]          uniqueidentifier NOT NULL,
        [company_id]  uniqueidentifier NOT NULL,
        [name]        nvarchar(200)    NOT NULL,
        [description] nvarchar(2000)   NOT NULL,
        [thumbnail]   nvarchar(500)    NULL,
        [status]      nvarchar(20)     NOT NULL,   -- Draft | Published | Archived
        [is_active]   bit              NOT NULL,
        [created_at]  datetime2(7)     NOT NULL,
        [created_by]  uniqueidentifier NOT NULL,
        [updated_at]  datetime2(7)     NULL,
        [updated_by]  uniqueidentifier NULL,

        CONSTRAINT [PK_course] PRIMARY KEY ([id]),

        CONSTRAINT [FK_course_companies_company_id]
            FOREIGN KEY ([company_id]) REFERENCES [hrm].[companies] ([id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_course_company_id] ON [hrm].[course] ([company_id]);
END;

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory]
                   WHERE [MigrationId] = N'20261011052123_AddCourseTable')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011052123_AddCourseTable', N'10.0.9');
END;

COMMIT TRANSACTION;
GO
