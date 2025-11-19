
-- =============================================
-- SQL Script to Remove Underscores from Column Names
-- =============================================
-- This script identifies all columns in the current database that:
-- 1. Contain underscores in their names
-- 2. Belong to tables that have 'erp' in their name
-- Then renames them by removing the underscores.
-- For example: In table 'ErpCustomers', column 'User_Id' becomes 'UserId'
-- =============================================

-- Create a temporary table to store information about columns that need to be renamed
DECLARE @ColumnRename TABLE (
    SchemaName NVARCHAR(128),    -- Schema name (e.g., 'dbo')
    TableName NVARCHAR(128),     -- Table name
    ColumnName NVARCHAR(128),    -- Original column name with underscores
    NewColumnName NVARCHAR(128)  -- New column name without underscores
)

-- Create a table to store generated SQL commands
DECLARE @GeneratedSQL TABLE (
    ID INT IDENTITY(1,1),
    CommandType NVARCHAR(50),
    SQLCommand NVARCHAR(MAX)
)

-- Find all columns in the current database that contain underscores
-- AND belong to tables with 'erp' in their name (case-insensitive)
INSERT INTO @ColumnRename (SchemaName, TableName, ColumnName, NewColumnName)
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    REPLACE(c.name, '_', '') AS NewColumnName
FROM 
    sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    c.name LIKE '%[_]%'
    AND t.name LIKE '%erp%'  -- Filter for tables containing 'erp'

-- Declare variables needed for processing each column
DECLARE @SchemaName NVARCHAR(128)
DECLARE @TableName NVARCHAR(128)
DECLARE @ColumnName NVARCHAR(128)
DECLARE @NewColumnName NVARCHAR(128)
DECLARE @SQL NVARCHAR(MAX)
DECLARE @DataType NVARCHAR(MAX)
DECLARE @IsNullable BIT
DECLARE @MaxLength INT
DECLARE @Precision INT
DECLARE @Scale INT

-- Create a cursor to iterate through each column that needs to be renamed
DECLARE column_cursor CURSOR FOR
SELECT SchemaName, TableName, ColumnName, NewColumnName FROM @ColumnRename

-- Open the cursor and get the first row
OPEN column_cursor
FETCH NEXT FROM column_cursor INTO @SchemaName, @TableName, @ColumnName, @NewColumnName

-- Loop through each column that needs to be renamed
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Get the data type and properties of the current column
    SELECT 
        @DataType = tp.name,
        @IsNullable = c.is_nullable,
        @MaxLength = c.max_length,
        @Precision = c.precision,
        @Scale = c.scale
    FROM 
        sys.columns c
        INNER JOIN sys.types tp ON c.user_type_id = tp.user_type_id
    WHERE 
        c.object_id = OBJECT_ID(QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName))
        AND c.name = @ColumnName

    -- Build the complete data type definition string
    DECLARE @DataTypeDefinition NVARCHAR(MAX)
    SET @DataTypeDefinition = @DataType

    -- Add appropriate size/length specifications based on the data type
    IF @DataType IN ('varchar', 'nvarchar', 'char', 'nchar')
    BEGIN
        IF @MaxLength = -1
            SET @DataTypeDefinition = @DataTypeDefinition + '(MAX)'
        ELSE IF @DataType IN ('varchar', 'char')
            SET @DataTypeDefinition = @DataTypeDefinition + '(' + CAST(@MaxLength AS NVARCHAR) + ')'
        ELSE
            SET @DataTypeDefinition = @DataTypeDefinition + '(' + CAST(@MaxLength/2 AS NVARCHAR) + ')'
    END
    ELSE IF @DataType IN ('decimal', 'numeric')
    BEGIN
        SET @DataTypeDefinition = @DataTypeDefinition + '(' + CAST(@Precision AS NVARCHAR) + ',' + CAST(@Scale AS NVARCHAR) + ')'
    END

    -- Add NULL or NOT NULL constraint
    IF @IsNullable = 1
        SET @DataTypeDefinition = @DataTypeDefinition + ' NULL'
    ELSE
        SET @DataTypeDefinition = @DataTypeDefinition + ' NOT NULL'

    -- Step 1: ALTER COLUMN statement
    SET @SQL = 'ALTER TABLE ' + QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName) + 
               ' ALTER COLUMN ' + QUOTENAME(@ColumnName) + ' ' + @DataTypeDefinition
    
    -- Store the ALTER command in our results table
    INSERT INTO @GeneratedSQL (CommandType, SQLCommand)
    VALUES ('ALTER', @SQL)
    
    -- Execute the ALTER command
    PRINT 'Executing: ' + @SQL
    EXEC sp_executesql @SQL

    -- Step 2: RENAME statement
    SET @SQL = 'EXEC sp_rename ''' + 
               QUOTENAME(@SchemaName) + '.' + 
               QUOTENAME(@TableName) + '.' + 
               QUOTENAME(@ColumnName) + ''', ''' + 
               @NewColumnName + ''', ''COLUMN'''
    
    -- Store the RENAME command in our results table
    INSERT INTO @GeneratedSQL (CommandType, SQLCommand)
    VALUES ('RENAME', @SQL)
    
    -- Execute the RENAME command
    PRINT 'Executing: ' + @SQL
    EXEC sp_executesql @SQL
    
    -- Get the next column to process
    FETCH NEXT FROM column_cursor INTO @SchemaName, @TableName, @ColumnName, @NewColumnName
END

-- Clean up the cursor
CLOSE column_cursor
DEALLOCATE column_cursor

-- Output a summary
DECLARE @RenamedCount INT
SELECT @RenamedCount = COUNT(*) FROM @ColumnRename
PRINT 'Successfully renamed ' + CAST(@RenamedCount AS NVARCHAR) + ' column(s) by removing underscores in tables containing ''erp''.'

-- Output all the generated SQL commands for review
SELECT ID,
    CommandType,
    SQLCommand
FROM 
    @GeneratedSQL
ORDER BY 
    ID

-- Output a summary table of all the renamed columns
SELECT 
    SchemaName,
    TableName,
    ColumnName AS OriginalColumnName,
    NewColumnName
FROM 
    @ColumnRename
ORDER BY 
    SchemaName,
    TableName,
    ColumnName


/*
Macsteel changes for column renaming:
--------------------------------------------
Stored Procedures:
	1. ErpProductLoadAllPagedNopAjaxFilters [executes from code]
	2. SpecialExclusionInclusion [executes from code]
	3. SP_B2BCustomerAccount_UpdateOrDeleteSpecialIncludeExcludes [executes from code]
	4. SP_B2BCustomerAccount_ImportSpecialIncludeExcludes [executes from code]
	5. UpdateB2BAccountFromParallelTableErpB2BAccount 
	6. B2BAccountNameToCustomerCompanySync
	7. UpdateB2BShipToAddressFromParallelTableErpB2BShipToAddress
	8. UpdateErpStockFromParallelTableParallel_ErpStock
	9. UpdateErp_Special_PriceFromParallelTableParallel_ErpAccountPricing
    10. sp_unpublish_products_without_prefilter [executes from report manager, nop4you]
    11. ErpUpdatePrefilterSpecificationAttributeJobSP [executes from agent, hourly]
    12. ErpPrefilterSpecificationAttributeRestoreSP [executes from code]
    13. All SPs from "3360-SP_Tasks_Nop4You.sql"
	14. ERPAdd0PriceForPerAccountProductPrice
 
Functions:
	1. ErpPercentageOfStock [executes from ajax filtere sp]
	2. ErpProductPrice [executes from ajax filter sp]

Triggers:
	1. ProductWarehouseInventory_UpdateDelete_Trigger
*/