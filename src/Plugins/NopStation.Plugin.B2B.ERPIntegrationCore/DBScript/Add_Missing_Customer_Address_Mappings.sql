-- =============================================
-- SQL Script to Add Missing Customer Address Mappings
-- =============================================
-- This script identifies customers whose billing or shipping addresses
-- are not properly mapped in the CustomerAddresses table, then adds
-- the missing mappings.
--
-- The script performs the following checks:
-- 1. Find customers with billing addresses not in mapping table
-- 2. Verify these addresses exist in the Address table
-- 3. Create missing mappings for valid billing addresses
-- 4. Repeat steps 1-3 for shipping addresses
-- =============================================

-- Variables to track counts
DECLARE @BillingCount INT;
DECLARE @ShippingCount INT;

-- First, identify missing billing address mappings and insert them
INSERT INTO [dbo].[CustomerAddresses]
    (Customer_Id, Address_Id)
SELECT 
    c.Id AS Customer_Id,
    c.BillingAddress_Id AS Address_Id
FROM 
    [dbo].[Customer] c
WHERE 
    c.BillingAddress_Id IS NOT NULL
    AND EXISTS (
        SELECT 1 FROM [dbo].[Address] a 
        WHERE a.Id = c.BillingAddress_Id
    )
    AND NOT EXISTS (
        SELECT 1 FROM [dbo].[CustomerAddresses] ca 
        WHERE ca.Customer_Id = c.Id AND ca.Address_Id = c.BillingAddress_Id
    );

-- Store billing count
SET @BillingCount = @@ROWCOUNT;

-- Next, identify missing shipping address mappings and insert them
INSERT INTO [dbo].[CustomerAddresses]
    (Customer_Id, Address_Id)
SELECT 
    c.Id AS Customer_Id,
    c.ShippingAddress_Id AS Address_Id
FROM 
    [dbo].[Customer] c
WHERE 
    c.ShippingAddress_Id IS NOT NULL
    AND EXISTS (
        SELECT 1 FROM [dbo].[Address] a 
        WHERE a.Id = c.ShippingAddress_Id
    )
    AND NOT EXISTS (
        SELECT 1 FROM [dbo].[CustomerAddresses] ca 
        WHERE ca.Customer_Id = c.Id AND ca.Address_Id = c.ShippingAddress_Id
    );

-- Store shipping count
SET @ShippingCount = @@ROWCOUNT;

-- Print results
PRINT 'Added ' + CAST(@BillingCount AS VARCHAR) + ' billing address mappings';
PRINT 'Added ' + CAST(@ShippingCount AS VARCHAR) + ' shipping address mappings';
PRINT 'Added ' + CAST((@BillingCount + @ShippingCount) AS VARCHAR) + ' total mappings';


GO
