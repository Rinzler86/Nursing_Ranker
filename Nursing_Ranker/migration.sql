IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Applicants] (
    [ApplicantId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [MiddleName] nvarchar(50) NULL,
    [LastName] nvarchar(100) NOT NULL,
    [WNumber] nvarchar(10) NULL,
    [WSCCGPA] decimal(18,2) NOT NULL,
    [NursingGPA] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [MissingRequirements] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Applicants] PRIMARY KEY ([ApplicantId])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(50) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [ProfilePicturePath] nvarchar(max) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [ApplicantCourses] (
    [ApplicantCourseId] int NOT NULL IDENTITY,
    [ApplicantId] int NOT NULL,
    [CourseName] nvarchar(max) NOT NULL,
    [Grade] nvarchar(max) NULL,
    [CompletionDate] datetime2 NULL,
    [Completed] bit NOT NULL,
    [RequiresRetake] bit NOT NULL,
    [PointsAwarded] int NOT NULL,
    CONSTRAINT [PK_ApplicantCourses] PRIMARY KEY ([ApplicantCourseId]),
    CONSTRAINT [FK_ApplicantCourses_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([ApplicantId]) ON DELETE CASCADE
);

CREATE TABLE [ApplicantRequirements] (
    [RequirementId] int NOT NULL IDENTITY,
    [ApplicantId] int NOT NULL,
    [TestName] nvarchar(max) NOT NULL,
    [Score] decimal(5,2) NOT NULL,
    [TestDate] datetime2 NULL,
    CONSTRAINT [PK_ApplicantRequirements] PRIMARY KEY ([RequirementId]),
    CONSTRAINT [FK_ApplicantRequirements_Applicants_ApplicantId] FOREIGN KEY ([ApplicantId]) REFERENCES [Applicants] ([ApplicantId]) ON DELETE CASCADE
);

CREATE INDEX [IX_ApplicantCourses_ApplicantId] ON [ApplicantCourses] ([ApplicantId]);

CREATE INDEX [IX_ApplicantRequirements_ApplicantId] ON [ApplicantRequirements] ([ApplicantId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250215062132_InitialCreate', N'9.0.1');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ApplicantRequirements]') AND [c].[name] = N'Score');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ApplicantRequirements] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [ApplicantRequirements] ALTER COLUMN [Score] decimal(5,2) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250215175700_MakeScoreNullable', N'9.0.1');

ALTER TABLE [ApplicantCourses] ADD [ExtraClassesPoints] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250215204013_AddExtraClassesPointsToApplicantCourse', N'9.0.1');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ApplicantCourses]') AND [c].[name] = N'ExtraClassesPoints');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ApplicantCourses] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [ApplicantCourses] DROP COLUMN [ExtraClassesPoints];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250216005846_RemoveExtraClassesPoints', N'9.0.1');

ALTER TABLE [Applicants] ADD [ExtraCredits] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250216031437_AddExtraCreditsToApplicant', N'9.0.1');

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Applicants]') AND [c].[name] = N'NursingGPA');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Applicants] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Applicants] ALTER COLUMN [NursingGPA] decimal(18,2) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Applicants]') AND [c].[name] = N'ExtraCredits');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Applicants] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Applicants] ADD DEFAULT 0 FOR [ExtraCredits];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250216062252_MakeNursingGPANullable', N'9.0.1');

COMMIT;
GO

