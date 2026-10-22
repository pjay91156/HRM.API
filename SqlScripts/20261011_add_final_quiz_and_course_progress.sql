-- Filtered unique indexes below require QUOTED_IDENTIFIER ON (the SSMS default; sqlcmd needs it set).
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    ALTER TABLE [hrm].[module_quiz] DROP CONSTRAINT [FK_module_quiz_course_module_module_id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    DROP INDEX [IX_module_quiz_module_id] ON [hrm].[module_quiz];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[hrm].[module_quiz]') AND [c].[name] = N'module_id');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [hrm].[module_quiz] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [hrm].[module_quiz] ALTER COLUMN [module_id] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    ALTER TABLE [hrm].[module_quiz] ADD [course_id] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    CREATE TABLE [hrm].[course_progress] (
        [id] uniqueidentifier NOT NULL,
        [course_id] uniqueidentifier NOT NULL,
        [employee_id] uniqueidentifier NOT NULL,
        [started_at] datetime2 NOT NULL,
        [completed_at] datetime2 NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] uniqueidentifier NOT NULL,
        [updated_at] datetime2 NULL,
        [updated_by] uniqueidentifier NULL,
        CONSTRAINT [PK_course_progress] PRIMARY KEY ([id]),
        CONSTRAINT [FK_course_progress_course_course_id] FOREIGN KEY ([course_id]) REFERENCES [hrm].[course] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_course_progress_employee_employee_id] FOREIGN KEY ([employee_id]) REFERENCES [hrm].[employee] ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_module_quiz_course_id] ON [hrm].[module_quiz] ([course_id]) WHERE [course_id] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_module_quiz_module_id] ON [hrm].[module_quiz] ([module_id]) WHERE [module_id] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    CREATE UNIQUE INDEX [IX_course_progress_course_id_employee_id] ON [hrm].[course_progress] ([course_id], [employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    CREATE INDEX [IX_course_progress_employee_id] ON [hrm].[course_progress] ([employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    ALTER TABLE [hrm].[module_quiz] ADD CONSTRAINT [FK_module_quiz_course_course_id] FOREIGN KEY ([course_id]) REFERENCES [hrm].[course] ([id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    ALTER TABLE [hrm].[module_quiz] ADD CONSTRAINT [FK_module_quiz_course_module_module_id] FOREIGN KEY ([module_id]) REFERENCES [hrm].[course_module] ([id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011071538_AddFinalQuizAndCourseProgress'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011071538_AddFinalQuizAndCourseProgress', N'10.0.9');
END;

COMMIT;
GO

