CREATE TABLE [dbo].[CringeUser] (
    [UserID]           INT           IDENTITY (1, 1) NOT NULL,
    [NickName]         NVARCHAR (50) NOT NULL,
    [AdminPrivelegies] INT           NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

