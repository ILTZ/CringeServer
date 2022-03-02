CREATE TABLE [dbo].[CringeDescription] (
    [CringeDescriptionID] INT           IDENTITY (1, 1) NOT NULL,
    [Description]         VARCHAR (MAX) NULL,
    CONSTRAINT [PK_CringeDescription] PRIMARY KEY CLUSTERED ([CringeDescriptionID] ASC)
);

