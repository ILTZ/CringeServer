CREATE TABLE [dbo].[ChatRoomGlobal] (
    [ChatRoomID] INT IDENTITY (1, 1) NOT NULL,
    [UserID]     INT NOT NULL,
    [MessageID]  INT NOT NULL,
    CONSTRAINT [PK_ChatRoomGlobal] PRIMARY KEY CLUSTERED ([ChatRoomID] ASC, [UserID] ASC, [MessageID] ASC),
    CONSTRAINT [FK_ChatRoomGlobal_Message] FOREIGN KEY ([MessageID]) REFERENCES [dbo].[Message] ([MessageID]),
    CONSTRAINT [FK_ChatRoomGlobal_User1] FOREIGN KEY ([UserID]) REFERENCES [dbo].[CringeUser] ([UserID])
);

