CREATE TABLE [dbo].[Groups]
(
    [GroupId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [GroupName] NVARCHAR(50) NULL, 
)

GO

CREATE INDEX [IX_Groups_GroupId] ON [dbo].[Groups] ([GroupId])
