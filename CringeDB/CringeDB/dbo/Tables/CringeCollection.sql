CREATE TABLE [dbo].[CringeCollection] (
    [UserID] INT NOT NULL,
    [Cringe] INT NOT NULL,
    CONSTRAINT [PK_CringeCollection_1] PRIMARY KEY CLUSTERED ([UserID] ASC, [Cringe] ASC),
    CONSTRAINT [FK_CringeCollection_Cringe] FOREIGN KEY ([Cringe]) REFERENCES [dbo].[Cringe] ([CringeID]),
    CONSTRAINT [FK_CringeCollection_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[CringeUser] ([UserID])
);

