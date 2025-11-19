IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CP_DomainImport]') AND type in (N'U'))
DROP TABLE [dbo].[CP_DomainImport];
GO
