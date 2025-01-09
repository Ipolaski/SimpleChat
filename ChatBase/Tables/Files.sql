CREATE TABLE [dbo].[Files]
(
    [FileId] UNIQUEIDENTIFIER NOT NULL, 
    [FileData] VARBINARY(MAX) NOT NULL, 
    [Date] Date
    CONSTRAINT [PK_Files] PRIMARY KEY ([FileId]) NOT NULL
)
GO


CREATE INDEX [IX_Files_FileId] ON [dbo].[Files] ([FileId])

GO
