CREATE TABLE [dbo].[Message] (
    [MessageID]      INT IDENTITY (1, 1) NOT NULL,
    [UserID]         INT NOT NULL,
    [MessageContent] INT NOT NULL,
    [wasDelivered]   INT NULL,
    [wasRead]        INT NULL,
    CONSTRAINT [PK_Message] PRIMARY KEY CLUSTERED ([MessageID] ASC),
    CONSTRAINT [FK_Message_MessageContent] FOREIGN KEY ([MessageContent]) REFERENCES [dbo].[MessageContent] ([MessageContentID]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Message_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[CringeUser] ([UserID]) ON DELETE CASCADE ON UPDATE CASCADE
);

