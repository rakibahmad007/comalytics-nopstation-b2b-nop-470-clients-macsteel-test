-- For tracking special products to send mails if the stock is removed or updated to 0
 CREATE TABLE [dbo].[SpecialCategoryProductStockDeleted] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [CategoryId] INT NOT NULL,
    [OperationTypeId] INT NOT NULL, -- 5 = Updated, 10 = Deleted
    
    -- Indexes
    INDEX IX_SpecialCategoryProductStockDeleted_ProductId ([ProductId]),
    INDEX IX_SpecialCategoryProductStockDeleted_Composite ([ProductId], [CategoryId])
);
GO

-- ProductWarehouseInventory_UpdateDelete_Trigger
CREATE OR ALTER TRIGGER [dbo].[ProductWarehouseInventory_UpdateDelete_Trigger]
ON [dbo].[ProductWarehouseInventory]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
 
    DECLARE @ProductId INT, @WarehouseId INT, @StockQuantity INT, @B2BSalesOrgId INT;
    DECLARE @OperationTypeUpdated INT = 5, @OperationTypeDeleted INT = 10;
    -- Cursor for update operations
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        DECLARE update_cursor CURSOR FOR 
        SELECT i.[ProductId], i.[WarehouseId], i.[StockQuantity]
        FROM inserted AS i;
        OPEN update_cursor;
        FETCH NEXT FROM update_cursor INTO @ProductId, @WarehouseId, @StockQuantity;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Check if StockQuantity > 0
            IF @StockQuantity > 0
            BEGIN
                DELETE d
                FROM [dbo].[SpecialCategoryProductStockDeleted] d
                JOIN [dbo].[Product_Category_Mapping] pcm ON d.ProductId = pcm.ProductId AND d.CategoryId = pcm.CategoryId
                JOIN [dbo].[Erp_Warehouse_Sales_Org_Map] swo ON swo.NopWarehouseId = @WarehouseId
                JOIN [dbo].[Erp_Sales_Org] so ON so.Id = swo.ErpSalesOrgId
                WHERE pcm.ProductId = @ProductId
                  AND pcm.CategoryId = so.SpecialsCategoryId
				  AND swo.IsB2CWarehouse = 0;
            END
            -- Check if StockQuantity = 0
            IF @StockQuantity = 0
            BEGIN
                -- Cursor to get B2BSalesOrgId
                DECLARE b2b_cursor CURSOR FOR
                SELECT ErpSalesOrgId
                FROM [Erp_Warehouse_Sales_Org_Map]
                WHERE NopWarehouseId = @WarehouseId AND IsB2CWarehouse = 0;
                OPEN b2b_cursor;
                FETCH NEXT FROM b2b_cursor INTO @B2BSalesOrgId;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    -- Check if all sibling warehouses have stock quantity 0
                    IF NOT EXISTS (
                        SELECT 1
                        FROM [ProductWarehouseInventory] AS pwi
                        JOIN [Erp_Warehouse_Sales_Org_Map] AS swo ON swo.NopWarehouseId = pwi.WarehouseId
                        WHERE pwi.ProductId = @ProductId
                        AND swo.ErpSalesOrgId = @B2BSalesOrgId
                        AND pwi.StockQuantity > 0
						AND swo.IsB2CWarehouse = 0
                    )
                    BEGIN
                        -- Fetch mappings from Product_Category_Mapping table
                        DECLARE @UpdatedProductMappings TABLE (
                            [ProductId] INT,
                            [WarehouseId] INT,
                            [CategoryId] INT
                        );
                        DECLARE @DeletedProductCategoryMapping TABLE (
                            [ProductId] INT,
                            [CategoryId] INT
                        );
                        INSERT INTO @UpdatedProductMappings ([ProductId], [WarehouseId], [CategoryId])
                        SELECT DISTINCT @ProductId, @WarehouseId, sc.[SpecialsCategoryId]
                        FROM [Erp_Sales_Org] AS sc
                        JOIN [Erp_Warehouse_Sales_Org_Map] AS swo ON sc.Id = swo.ErpSalesOrgId
                        WHERE swo.NopWarehouseId = @WarehouseId AND swo.IsB2CWarehouse = 0
                        AND swo.ErpSalesOrgId = @B2BSalesOrgId;
                        INSERT INTO @DeletedProductCategoryMapping ([ProductId], [CategoryId])
                        SELECT pcm.[ProductId], pcm.[CategoryId]
                        FROM [dbo].[Product_Category_Mapping] pcm
                        JOIN @UpdatedProductMappings um ON pcm.[ProductId] = um.[ProductId] AND pcm.[CategoryId] = um.[CategoryId];
                        DELETE pcm
                        FROM [dbo].[Product_Category_Mapping] pcm
                        JOIN @UpdatedProductMappings um ON pcm.[ProductId] = um.[ProductId] AND pcm.[CategoryId] = um.[CategoryId];
                        
                        -- Track info to send email later
                        -- Update existing rows
                        UPDATE d
                        SET OperationTypeId = @OperationTypeUpdated
                        FROM [dbo].[SpecialCategoryProductStockDeleted] d
                        JOIN @DeletedProductCategoryMapping dm
                            ON d.ProductId = dm.ProductId AND d.CategoryId = dm.CategoryId;

                        -- Insert rows that don't exist
                        INSERT INTO [dbo].[SpecialCategoryProductStockDeleted] ([ProductId], [CategoryId], [OperationTypeId])
                        SELECT dm.ProductId, dm.CategoryId, @OperationTypeUpdated
                        FROM @DeletedProductCategoryMapping dm
                        WHERE NOT EXISTS (
                            SELECT 1
                            FROM [dbo].[SpecialCategoryProductStockDeleted] d
                            WHERE d.ProductId = dm.ProductId AND d.CategoryId = dm.CategoryId
                        );
                    END;
                    FETCH NEXT FROM b2b_cursor INTO @B2BSalesOrgId;
                END;
                CLOSE b2b_cursor;
                DEALLOCATE b2b_cursor;
            END;
            FETCH NEXT FROM update_cursor INTO @ProductId, @WarehouseId, @StockQuantity;
        END;
        CLOSE update_cursor;
        DEALLOCATE update_cursor;
    END;
 
    -- Cursor for delete operations
    IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
    BEGIN
        DECLARE delete_cursor CURSOR FOR 
        SELECT d.[ProductId], d.[WarehouseId]
        FROM deleted AS d;
        OPEN delete_cursor;
        FETCH NEXT FROM delete_cursor INTO @ProductId, @WarehouseId;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Cursor to get B2BSalesOrgId
            DECLARE b2b_cursor CURSOR FOR
            SELECT ErpSalesOrgId
            FROM [Erp_Warehouse_Sales_Org_Map]
            WHERE NopWarehouseId = @WarehouseId AND IsB2CWarehouse = 0;
            OPEN b2b_cursor;
            FETCH NEXT FROM b2b_cursor INTO @B2BSalesOrgId;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                -- Check if all sibling warehouses have stock quantity 0
                IF NOT EXISTS (
                    SELECT 1
                    FROM [ProductWarehouseInventory] AS pwi
                    JOIN [Erp_Warehouse_Sales_Org_Map] AS swo ON swo.NopWarehouseId = pwi.WarehouseId
                    WHERE pwi.ProductId = @ProductId
                    AND swo.ErpSalesOrgId = @B2BSalesOrgId
                    AND pwi.StockQuantity > 0
					AND swo.IsB2CWarehouse = 0
                )
                BEGIN
                    -- Fetch mappings from Product_Category_Mapping table
                    DECLARE @DeletedProductMappings TABLE (
                        [ProductId] INT,
                        [WarehouseId] INT,
                        [CategoryId] INT
                    );
                    DECLARE @DeletedProductCategoryMappings TABLE (
                        [ProductId] INT,
                        [CategoryId] INT
                    );
                    INSERT INTO @DeletedProductMappings ([ProductId], [WarehouseId], [CategoryId])
                    SELECT DISTINCT @ProductId, @WarehouseId, sc.[SpecialsCategoryId]
                    FROM [Erp_Sales_Org] AS sc
                    JOIN [Erp_Warehouse_Sales_Org_Map] AS swo ON sc.[Id] = swo.ErpSalesOrgId
                    WHERE swo.NopWarehouseId = @WarehouseId
                    AND swo.ErpSalesOrgId = @B2BSalesOrgId
					AND swo.IsB2CWarehouse = 0;
                    INSERT INTO @DeletedProductCategoryMappings ([ProductId], [CategoryId])
                    SELECT pcm.[ProductId], pcm.[CategoryId]
                    FROM [dbo].[Product_Category_Mapping] pcm
                    JOIN @DeletedProductMappings um ON pcm.[ProductId] = um.[ProductId] AND pcm.[CategoryId] = um.[CategoryId];
                    DELETE pcm
                    FROM [dbo].[Product_Category_Mapping] pcm
                    JOIN @DeletedProductMappings um ON pcm.[ProductId] = um.[ProductId] AND pcm.[CategoryId] = um.[CategoryId];

                    -- Track info to send email later
                    -- Update existing rows
                    UPDATE d
                    SET OperationTypeId = @OperationTypeDeleted
                    FROM [dbo].[SpecialCategoryProductStockDeleted] d
                    JOIN @DeletedProductCategoryMappings dm
                        ON d.ProductId = dm.ProductId AND d.CategoryId = dm.CategoryId;

                    -- Insert rows that don't exist
                    INSERT INTO [dbo].[SpecialCategoryProductStockDeleted] ([ProductId], [CategoryId], [OperationTypeId])
                    SELECT dm.ProductId, dm.CategoryId, @OperationTypeDeleted
                    FROM @DeletedProductCategoryMappings dm
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM [dbo].[SpecialCategoryProductStockDeleted] d
                        WHERE d.ProductId = dm.ProductId AND d.CategoryId = dm.CategoryId
                    );
                END;
                FETCH NEXT FROM b2b_cursor INTO @B2BSalesOrgId;
            END;
            CLOSE b2b_cursor;
            DEALLOCATE b2b_cursor;
            FETCH NEXT FROM delete_cursor INTO @ProductId, @WarehouseId;
        END;
        CLOSE delete_cursor;
        DEALLOCATE delete_cursor;
    END;
END;
GO

-- SP for sending mail if special category's product is deleted
CREATE OR ALTER PROCEDURE [dbo].[SP_SendEmailForStockUpdateDeleteOfSpecialCategoryProducts]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EmailBody NVARCHAR(MAX) = '';
    DECLARE @UpdatedCount INT = 0;
    DECLARE @DeletedCount INT = 0;
    DECLARE @TotalCount INT = 0;
    DECLARE @OperationTypeUpdated INT = 5, @OperationTypeDeleted INT = 10;

    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check counts for each operation type
        SELECT @UpdatedCount = COUNT(*) FROM [dbo].[SpecialCategoryProductStockDeleted] WHERE [OperationTypeId] = @OperationTypeUpdated; -- Updated
        SELECT @DeletedCount = COUNT(*) FROM [dbo].[SpecialCategoryProductStockDeleted] WHERE [OperationTypeId] = @OperationTypeDeleted; -- Deleted
        SET @TotalCount = @UpdatedCount + @DeletedCount;
        
        -- If no records, exit gracefully
        IF @TotalCount = 0
        BEGIN
            COMMIT TRANSACTION;
            RETURN;
        END;
        
        -- Build email header (HTML)
        SET @EmailBody = '<p>Materials on special were out of stock and removed from the specials category</p><br>';
        
        -- UPDATED PRODUCTS SECTION
        IF @UpdatedCount > 0
        BEGIN
            SET @EmailBody = @EmailBody + '<b>UPDATED - Materials removed from specials (' + CAST(@UpdatedCount AS NVARCHAR(10)) + ' items):</b><br>';
            SET @EmailBody = @EmailBody + '================================================================<br>';
            
            DECLARE @ProductSKU NVARCHAR(400);
            DECLARE @CategoryName NVARCHAR(400);
            
            DECLARE updated_cursor CURSOR FOR
            SELECT p.[SKU], c.[Name]
            FROM [dbo].[SpecialCategoryProductStockDeleted] spud
            INNER JOIN [dbo].[Product] p ON p.[Id] = spud.[ProductId]
            INNER JOIN [dbo].[Category] c ON c.[Id] = spud.[CategoryId]
            WHERE spud.[OperationTypeId] = @OperationTypeUpdated -- Updated
            ORDER BY p.[SKU];
            
            OPEN updated_cursor;
            FETCH NEXT FROM updated_cursor INTO @ProductSKU, @CategoryName;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @EmailBody = @EmailBody + 'Material number: ' + ISNULL(@ProductSKU, 'N/A') + 
                    ' was removed from specials category: ' + ISNULL(@CategoryName, 'N/A') + '<br>';
                FETCH NEXT FROM updated_cursor INTO @ProductSKU, @CategoryName;
            END;
            
            CLOSE updated_cursor;
            DEALLOCATE updated_cursor;

            SET @EmailBody = @EmailBody + '<br><br>'; -- extra spacing
        END;
        
        -- DELETED PRODUCTS SECTION
        IF @DeletedCount > 0
        BEGIN
            SET @EmailBody = @EmailBody + '<b>DELETED - Materials removed from specials (' + CAST(@DeletedCount AS NVARCHAR(10)) + ' items):</b><br>';
            SET @EmailBody = @EmailBody + '================================================================<br>';
            
            DECLARE deleted_cursor CURSOR FOR
            SELECT p.[SKU], c.[Name]
            FROM [dbo].[SpecialCategoryProductStockDeleted] spud
            INNER JOIN [dbo].[Product] p ON p.[Id] = spud.[ProductId]
            INNER JOIN [dbo].[Category] c ON c.[Id] = spud.[CategoryId]
            WHERE spud.[OperationTypeId] = @OperationTypeDeleted -- Deleted
            ORDER BY p.[SKU];
            
            OPEN deleted_cursor;
            FETCH NEXT FROM deleted_cursor INTO @ProductSKU, @CategoryName;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @EmailBody = @EmailBody + 'Material number: ' + ISNULL(@ProductSKU, 'N/A') + 
                    ' was removed from specials category: ' + ISNULL(@CategoryName, 'N/A') + '<br>';
                FETCH NEXT FROM deleted_cursor INTO @ProductSKU, @CategoryName;
            END;
            
            CLOSE deleted_cursor;
            DEALLOCATE deleted_cursor;

            SET @EmailBody = @EmailBody + '<br><br>'; -- spacing before footer
        END;
        
        -- Email footer
        SET @EmailBody = @EmailBody + '<b>Total affected materials: </b>' + CAST(@TotalCount AS NVARCHAR(10)) + '<br>';
        SET @EmailBody = @EmailBody + 'Generated on: ' + CONVERT(NVARCHAR(19), GETUTCDATE(), 120) + ' UTC';
        
        -- Insert into QueuedEmail
        INSERT INTO [dbo].[QueuedEmail] (
            PriorityId, [From], FromName, [To], ToName, ReplyTo, ReplyToName, CC, Bcc, [Subject],
            Body, AttachmentFilePath, AttachmentFileName, AttachedDownloadId, CreatedOnUtc,
            DontSendBeforeDateUtc, SentTries, SentOnUtc, EmailAccountId
        )
        VALUES (
            5, 
            'ecommerce@macsteel.co.za', 
            'Macsteel''s Online Shop', 
            'ecommerce@macsteel.co.za', 
            'Macsteel''s Online Shop', 
            NULL, NULL, '', NULL, 
            'Materials on special were out of stock and removed from the specials category',
            @EmailBody,
            NULL, NULL, 0, GETUTCDATE(), NULL, 0, NULL, 1
        );
        
        -- Clean up processed records
        DELETE FROM [dbo].[SpecialCategoryProductStockDeleted];
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH;
END;
GO