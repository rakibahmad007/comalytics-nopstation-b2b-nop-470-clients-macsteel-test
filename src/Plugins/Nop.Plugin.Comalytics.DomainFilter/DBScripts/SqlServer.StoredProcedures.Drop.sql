IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('[dbo].[CP_DomainImportProcedure]'))
DROP PROCEDURE [dbo].[CP_DomainImportProcedure];
GO
