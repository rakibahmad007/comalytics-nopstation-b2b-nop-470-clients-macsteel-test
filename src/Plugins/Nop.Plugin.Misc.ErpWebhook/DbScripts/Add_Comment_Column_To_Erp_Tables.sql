ALTER TABLE [dbo].[Erp_Account]
ADD [Comment] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[Erp_Special_Price]
ADD [Comment] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[Erp_ShipToAddress]
ADD [Comment] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[Parallel_ErpStock]
ADD WarehouseCode NVARCHAR(100);