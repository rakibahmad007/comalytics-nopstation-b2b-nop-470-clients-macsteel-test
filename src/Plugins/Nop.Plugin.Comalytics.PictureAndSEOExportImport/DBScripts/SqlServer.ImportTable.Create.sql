IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductPictureAndSEOUpdateExcelImport' and xtype='U')
CREATE TABLE [dbo].[ProductPictureAndSEOUpdateExcelImport](
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ProductId] [int] NOT NULL,
    [Name] [nvarchar](Max) NOT NULL,
    [Sku] [nvarchar](Max) NOT NULL,
    [MetaKeywords] [nvarchar](Max),
    [MetaDescription] [nvarchar](Max),
    [MetaTitle] [nvarchar](Max),
    [SeName] [nvarchar](Max),
    [Picture1Id] [int] NOT NULL,
    [Picture1Title] [nvarchar](Max),
    [Picture1Alt] [nvarchar](Max),
    [Picture1Url] [nvarchar](Max),
    
    [Picture2Id] [int] NOT NULL,
    [Picture2Title] [nvarchar](Max),
    [Picture2Alt] [nvarchar](Max),
    [Picture2Url] [nvarchar](Max),
    
    [Picture3Id] [int] NOT NULL,
    [Picture3Title] [nvarchar](Max),
    [Picture3Alt] [nvarchar](Max),
    [Picture3Url] [nvarchar](Max),
    
    )
