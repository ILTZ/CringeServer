CREATE TABLE [dbo].[CringeImage] (
    [CringeImageID] INT   IDENTITY (1, 1) NOT NULL,
    [Image]         IMAGE NULL,
    CONSTRAINT [PK_CringeImage] PRIMARY KEY CLUSTERED ([CringeImageID] ASC)
);

