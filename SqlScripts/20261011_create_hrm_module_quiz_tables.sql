BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_progress] (
        [id] uniqueidentifier NOT NULL,
        [module_id] uniqueidentifier NOT NULL,
        [employee_id] uniqueidentifier NOT NULL,
        [learning_completed_at] datetime2 NULL,
        [quiz_passed_at] datetime2 NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] uniqueidentifier NOT NULL,
        [updated_at] datetime2 NULL,
        [updated_by] uniqueidentifier NULL,
        CONSTRAINT [PK_module_progress] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_progress_course_module_module_id] FOREIGN KEY ([module_id]) REFERENCES [hrm].[course_module] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_module_progress_employee_employee_id] FOREIGN KEY ([employee_id]) REFERENCES [hrm].[employee] ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_quiz] (
        [id] uniqueidentifier NOT NULL,
        [module_id] uniqueidentifier NOT NULL,
        [passing_percentage] int NOT NULL,
        [max_attempts] int NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] uniqueidentifier NOT NULL,
        [updated_at] datetime2 NULL,
        [updated_by] uniqueidentifier NULL,
        CONSTRAINT [PK_module_quiz] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_quiz_course_module_module_id] FOREIGN KEY ([module_id]) REFERENCES [hrm].[course_module] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_quiz_attempt] (
        [id] uniqueidentifier NOT NULL,
        [quiz_id] uniqueidentifier NOT NULL,
        [employee_id] uniqueidentifier NOT NULL,
        [attempt_number] int NOT NULL,
        [total_questions] int NOT NULL,
        [correct_answers] int NOT NULL,
        [score_percentage] decimal(5,2) NOT NULL,
        [passing_percentage] int NOT NULL,
        [is_passed] bit NOT NULL,
        [started_at] datetime2 NOT NULL,
        [submitted_at] datetime2 NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] uniqueidentifier NOT NULL,
        [updated_at] datetime2 NULL,
        [updated_by] uniqueidentifier NULL,
        CONSTRAINT [PK_module_quiz_attempt] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_quiz_attempt_employee_employee_id] FOREIGN KEY ([employee_id]) REFERENCES [hrm].[employee] ([id]),
        CONSTRAINT [FK_module_quiz_attempt_module_quiz_quiz_id] FOREIGN KEY ([quiz_id]) REFERENCES [hrm].[module_quiz] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_quiz_question] (
        [id] uniqueidentifier NOT NULL,
        [quiz_id] uniqueidentifier NOT NULL,
        [question_text] nvarchar(1000) NOT NULL,
        [is_multiple_answer] bit NOT NULL,
        [sort_order] int NOT NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] uniqueidentifier NOT NULL,
        [updated_at] datetime2 NULL,
        [updated_by] uniqueidentifier NULL,
        CONSTRAINT [PK_module_quiz_question] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_quiz_question_module_quiz_quiz_id] FOREIGN KEY ([quiz_id]) REFERENCES [hrm].[module_quiz] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_quiz_answer] (
        [id] uniqueidentifier NOT NULL,
        [attempt_id] uniqueidentifier NOT NULL,
        [question_id] uniqueidentifier NOT NULL,
        [question_text] nvarchar(1000) NOT NULL,
        [is_multiple_answer] bit NOT NULL,
        [sort_order] int NOT NULL,
        [is_correct] bit NOT NULL,
        CONSTRAINT [PK_module_quiz_answer] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_quiz_answer_module_quiz_attempt_attempt_id] FOREIGN KEY ([attempt_id]) REFERENCES [hrm].[module_quiz_attempt] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_module_quiz_answer_module_quiz_question_question_id] FOREIGN KEY ([question_id]) REFERENCES [hrm].[module_quiz_question] ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_quiz_option] (
        [id] uniqueidentifier NOT NULL,
        [question_id] uniqueidentifier NOT NULL,
        [option_text] nvarchar(500) NOT NULL,
        [is_correct] bit NOT NULL,
        [sort_order] int NOT NULL,
        [is_active] bit NOT NULL,
        [created_at] datetime2 NOT NULL,
        [created_by] uniqueidentifier NOT NULL,
        [updated_at] datetime2 NULL,
        [updated_by] uniqueidentifier NULL,
        CONSTRAINT [PK_module_quiz_option] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_quiz_option_module_quiz_question_question_id] FOREIGN KEY ([question_id]) REFERENCES [hrm].[module_quiz_question] ([id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE TABLE [hrm].[module_quiz_answer_option] (
        [id] uniqueidentifier NOT NULL,
        [answer_id] uniqueidentifier NOT NULL,
        [option_id] uniqueidentifier NOT NULL,
        [option_text] nvarchar(500) NOT NULL,
        [is_correct] bit NOT NULL,
        [is_selected] bit NOT NULL,
        [sort_order] int NOT NULL,
        CONSTRAINT [PK_module_quiz_answer_option] PRIMARY KEY ([id]),
        CONSTRAINT [FK_module_quiz_answer_option_module_quiz_answer_answer_id] FOREIGN KEY ([answer_id]) REFERENCES [hrm].[module_quiz_answer] ([id]) ON DELETE CASCADE,
        CONSTRAINT [FK_module_quiz_answer_option_module_quiz_option_option_id] FOREIGN KEY ([option_id]) REFERENCES [hrm].[module_quiz_option] ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE INDEX [IX_module_progress_employee_id] ON [hrm].[module_progress] ([employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_progress_module_id_employee_id] ON [hrm].[module_progress] ([module_id], [employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_quiz_module_id] ON [hrm].[module_quiz] ([module_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_quiz_answer_attempt_id_question_id] ON [hrm].[module_quiz_answer] ([attempt_id], [question_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE INDEX [IX_module_quiz_answer_question_id] ON [hrm].[module_quiz_answer] ([question_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_quiz_answer_option_answer_id_option_id] ON [hrm].[module_quiz_answer_option] ([answer_id], [option_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE INDEX [IX_module_quiz_answer_option_option_id] ON [hrm].[module_quiz_answer_option] ([option_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE INDEX [IX_module_quiz_attempt_employee_id] ON [hrm].[module_quiz_attempt] ([employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_quiz_attempt_quiz_id_employee_id_attempt_number] ON [hrm].[module_quiz_attempt] ([quiz_id], [employee_id], [attempt_number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE INDEX [IX_module_quiz_option_question_id_sort_order] ON [hrm].[module_quiz_option] ([question_id], [sort_order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    CREATE INDEX [IX_module_quiz_question_quiz_id_sort_order] ON [hrm].[module_quiz_question] ([quiz_id], [sort_order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011064829_AddModuleQuizTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011064829_AddModuleQuizTables', N'10.0.9');
END;

COMMIT;
GO

