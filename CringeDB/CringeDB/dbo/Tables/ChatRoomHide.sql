CREATE TABLE [dbo].[ChatRoomHide] (
    [ChatRoomID] INT IDENTITY (1, 1) NOT NULL,
    [UserID]     INT NOT NULL,
    [Message]    INT NOT NULL,
    CONSTRAINT [PK_ChatRommHide] PRIMARY KEY CLUSTERED ([ChatRoomID] ASC, [UserID] ASC, [Message] ASC),
    CONSTRAINT [FK_ChatRoomHide_CringeUser] FOREIGN KEY ([UserID]) REFERENCES [dbo].[CringeUser] ([UserID]),
    CONSTRAINT [FK_ChatRoomHide_Message] FOREIGN KEY ([Message]) REFERENCES [dbo].[Message] ([MessageID])
);

