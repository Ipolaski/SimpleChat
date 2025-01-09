CREATE TABLE [dbo].[GroupConnections]
(
    [ConnectionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [GroupId] UNIQUEIDENTIFIER NOT NULL, 
    [UserId] UNIQUEIDENTIFIER NOT NULL, 
    [IsAdmin] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_GroupConnections_Groups] FOREIGN KEY ([GroupId]) REFERENCES [Groups]([GroupId]), 
    CONSTRAINT [FK_GroupConnections_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
)

GO

CREATE UNIQUE INDEX [IX_Table1_Column] ON [dbo].[GroupConnections] ([ConnectionId])
