IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CP_DomainImport' and xtype='U')
CREATE TABLE [dbo].[CP_DomainImport](
	[Id] [nvarchar](100) NULL,
	[DomainOrEmailName] [nvarchar](max) NULL,
	[TypeId] [nvarchar](max) NULL,
	[IsActive] [nvarchar](max) NULL
)
GO
