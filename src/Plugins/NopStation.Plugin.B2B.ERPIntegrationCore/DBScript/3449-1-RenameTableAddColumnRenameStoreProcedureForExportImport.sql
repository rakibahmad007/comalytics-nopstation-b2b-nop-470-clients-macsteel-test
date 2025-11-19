--Erp Nop User
ALTER TABLE [dbo].[B2BUserInformationImport]
ADD [ErpUserType] NVARCHAR(100) NULL;

EXEC sp_rename 'dbo.B2BUserInformationImport', 'ErpNopUserImport';

EXEC sp_rename 'dbo.B2BUserInformationImportProcedure', 'ErpNopUserImportProcedure';



-- Erp Accounts
ALTER TABLE [Nop_Macsteel_Test_Comalytics_470].[dbo].[B2BAccountImport]
ADD [IsDefaultPaymentAccount] NVARCHAR(10) NULL;

EXEC sp_rename 
    '[dbo].[B2BAccountImport].[B2BAccountStatusTypeId]',  -- current column name
    'ErpAccountStatusTypeId',                              -- new column name
    'COLUMN';

EXEC sp_rename 'dbo.B2BAccountImportProcedure', 'ErpAccountImportProcedure';

EXEC sp_rename 'dbo.B2BAccountImport', 'ErpAccountImport';



--Erp Ship to Address

EXEC sp_rename 'B2BShipToAddressImportProcedure', 'ErpShipToAddressImportProcedure';

EXEC sp_rename 'dbo.B2BShipToAddressImport', 'ErpShipToAddressImport';
