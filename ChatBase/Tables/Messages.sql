CREATE TABLE [dbo].[Messages]
(
    [MessageId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [GroupId] UNIQUEIDENTIFIER NOT NULL, 
    [UserId] UNIQUEIDENTIFIER NOT NULL, 
    [Text] TEXT NULL, 
    [FileId] UNIQUEIDENTIFIER NULL, 
    [DateTime] DATETIME2 NOT NULL, 
    CONSTRAINT [FK_Messages_Groups] FOREIGN KEY ([GroupId]) REFERENCES [Groups]([GroupId]), 
    CONSTRAINT [FK_Messages_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]), 
    CONSTRAINT [FK_Messages_Files] FOREIGN KEY ([FileId]) REFERENCES [Files]([FileId])
)

GO

CREATE INDEX [IX_Messages_MessageId] ON [dbo].[Messages] ([MessageId])
