ALTER TABLE [dbo].[B2BAccount]
ADD [Comment] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[B2BPerAccountProductPricing]
ADD [Comment] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[B2BShipToAddress]
ADD [Comment] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[ErpStock]
ADD WarehouseCode NVARCHAR(100);