CREATE TABLE [dbo].[MessageContent] (
    [MessageContentID] INT           IDENTITY (1, 1) NOT NULL,
    [MessageText]      VARCHAR (MAX) NULL,
    [MessageImage]     IMAGE         NOT NULL,
    CONSTRAINT [PK_MessageContent] PRIMARY KEY CLUSTERED ([MessageContentID] ASC)
);

