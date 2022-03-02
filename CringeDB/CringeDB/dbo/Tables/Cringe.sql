CREATE TABLE [dbo].[Cringe] (
    [CringeID]          INT IDENTITY (1, 1) NOT NULL,
    [Image]             INT NULL,
    [CringeDescription] INT NULL,
    CONSTRAINT [PK_Cringe] PRIMARY KEY CLUSTERED ([CringeID] ASC),
    CONSTRAINT [FK_Cringe_CringeDescription] FOREIGN KEY ([CringeDescription]) REFERENCES [dbo].[CringeDescription] ([CringeDescriptionID]),
    CONSTRAINT [FK_Cringe_CringeImage] FOREIGN KEY ([Image]) REFERENCES [dbo].[CringeImage] ([CringeImageID])
);

