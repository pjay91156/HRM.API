/*
    Course Module Management - creates hrm.course_module

    Run in SSMS against the HRM database (HumanResourceManagement), after
    20261011_create_hrm_course.sql. Safe to run more than once.

    This creates exactly what the EF Core migration 20261011063401_AddCourseModuleTable
    creates. Use either this script or `dotnet ef database update`, not both by hand:
    when the EF migration history table exists, the script records the migration
    so a later `dotnet ef database update` skips it.
*/

SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'[hrm].[course_module]', N'U') IS NULL
BEGIN
    CREATE TABLE [hrm].[course_module]
    (
        [id]          uniqueidentifier NOT NULL,
        [course_id]   uniqueidentifier NOT NULL,
        [title]       nvarchar(200)    NOT NULL,
        [description] nvarchar(2000)   NULL,
        [content]     nvarchar(max)    NULL,
        [video_path]  nvarchar(500)    NULL,
        [sort_order]  int              NOT NULL,
        [is_active]   bit              NOT NULL,
        [created_at]  datetime2(7)     NOT NULL,
        [created_by]  uniqueidentifier NOT NULL,
        [updated_at]  datetime2(7)     NULL,
        [updated_by]  uniqueidentifier NULL,

        CONSTRAINT [PK_course_module] PRIMARY KEY ([id]),

        CONSTRAINT [FK_course_module_course_course_id]
            FOREIGN KEY ([course_id]) REFERENCES [hrm].[course] ([id])
            ON DELETE CASCADE
    );

    CREATE INDEX [IX_course_module_course_id_sort_order]
        ON [hrm].[course_module] ([course_id], [sort_order]);
END;

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory]
                   WHERE [MigrationId] = N'20261011063401_AddCourseModuleTable')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011063401_AddCourseModuleTable', N'10.0.9');
END;

COMMIT TRANSACTION;
GO
