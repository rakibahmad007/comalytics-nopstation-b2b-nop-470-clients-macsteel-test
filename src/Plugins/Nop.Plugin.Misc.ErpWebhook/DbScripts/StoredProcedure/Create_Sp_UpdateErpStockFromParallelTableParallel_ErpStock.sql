CREATE OR ALTER PROCEDURE [dbo].[UpdateErpStockFromParallelTableParallel_ErpStock]
AS
BEGIN
    DECLARE @Id int,
			@Sku nvarchar(max),
            @SalesOrganisationCode nvarchar(max),
            @TotalOnHand int,
            @UOM nvarchar(max),
            @Weight decimal(18, 2),
            @WarehouseCode nvarchar(100),
            @NopWareHouseId int,
            @ProductId int,
            @SalesOrgId int,
			@CurrentStockQuantity int,
			@CurrentReserveQuantity int,
			@UOMSpecifiedId int,
			@QuantityAdjustment int;

    DECLARE cursor_UpdateB2BStock CURSOR FOR
    SELECT 
		es.[Id],
        es.[Sku],
        es.[SalesOrganisationCode],
        es.[TotalOnHand],
        es.[UOM],
        es.[Weight],
        es.[WarehouseCode]
    FROM [Parallel_ErpStock] es WITH (NOLOCK)
    WHERE es.[IsUpdated] = 0;

    OPEN cursor_UpdateB2BStock;
    FETCH NEXT FROM cursor_UpdateB2BStock INTO @Id, @Sku, @SalesOrganisationCode, @TotalOnHand, @UOM, @Weight, @WarehouseCode;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ProductId = (Select TOP 1 Id From Product Where Sku = @Sku and Published = 1 and Deleted = 0)
		SET @SalesOrgId =  (Select TOP 1 Id From Erp_Sales_Org Where Code = @SalesOrganisationCode and IsActive = 1 and IsDeleted = 0)
		
        SELECT TOP 1 @NopWareHouseId = wsom.NopWarehouseId
		FROM Erp_Warehouse_Sales_Org_Map wsom
		WHERE wsom.ErpSalesOrgId = @SalesOrgId
		AND wsom.WarehouseCode = @WarehouseCode
        AND wsom.IsB2CWarehouse = 0;

        -- If no value found for B2BSalesOrgWarehouse, check for B2CSalesOrgWarehouse
        IF @NopWareHouseId IS NULL
        BEGIN
            SELECT TOP 1 @NopWareHouseId = wsom.NopWarehouseId
		    FROM Erp_Warehouse_Sales_Org_Map wsom
		    WHERE wsom.ErpSalesOrgId = @SalesOrgId
		    AND wsom.WarehouseCode = @WarehouseCode
            AND wsom.IsB2CWarehouse = 1;
        END

		IF EXISTS (
		    SELECT 1
		    FROM [dbo].[ProductWarehouseInventory]
		    WHERE [WarehouseId] = @NopWareHouseId 
		    AND [ProductId] = @ProductId 
		)
		BEGIN
		    SELECT TOP 1 
		        @CurrentStockQuantity = ISNULL(StockQuantity, 0),
		        @CurrentReserveQuantity = ISNULL(ReservedQuantity, 0)
		    FROM [dbo].[ProductWarehouseInventory]
		    WHERE [WarehouseId] = @NopWareHouseId 
		    AND [ProductId] = @ProductId
		
		    IF @CurrentStockQuantity != @TotalOnHand OR @CurrentReserveQuantity > 0
		    BEGIN
				UPDATE [ProductWarehouseInventory] SET [StockQuantity] = @TotalOnHand Where ProductId = @ProductId and WarehouseId = @NopWareHouseId

		        SET @QuantityAdjustment = @TotalOnHand - @CurrentStockQuantity
				INSERT INTO StockQuantityHistory (QuantityAdjustment, StockQuantity, CreatedOnUtc, ProductId, WarehouseId, Message)
				VALUES (@QuantityAdjustment, @TotalOnHand, GETUTCDATE(), @ProductId, @NopWareHouseId, CONCAT('Updated from Webhook - ', GETUTCDATE()));
		    END

			IF @Weight > 0
			BEGIN
				UPDATE Product SET [Weight] = @Weight Where Id = @ProductId
			END

			IF @UOM IS NOT NUll AND @UOM != ''
			BEGIN
				SET @UOMSpecifiedId = (SELECT TOP 1 Id FROM SpecificationAttribute WHERE [Name] = 'UOM')
				IF @UOMSpecifiedId IS NOT NULL
				BEGIN
					IF NOT EXISTS(
						SELECT 1
						FROM SpecificationAttributeOption
						WHERE SpecificationAttributeId = @UOMSpecifiedId AND [Name] = @UOM
					)
					BEGIN
						 -- Insert the UOM option into SpecificationAttributeOption table
			           INSERT INTO SpecificationAttributeOption (SpecificationAttributeId, [Name], [ColorSquaresRgb] ,[DisplayOrder])
			           VALUES (@UOMSpecifiedId, @UOM, NULL, 0)
					   
			           -- Get the Id of the newly inserted UOM option
			           DECLARE @UOMOptionId INT
			           SET @UOMOptionId = SCOPE_IDENTITY()
					   
			           -- Insert into mappings table (Product_SpecificationAttribute_Mapping)
			           IF @UOMOptionId IS NOT NULL
			           BEGIN
			               INSERT INTO Product_SpecificationAttribute_Mapping (ProductId, SpecificationAttributeOptionId, AttributeTypeId, AllowFiltering, ShowOnProductPage, DisplayOrder)
			               VALUES (@ProductId, @UOMOptionId, 0, 1, 1, 0) --AttributeTypeId 0 represents 'Option' type
			           END
					END
				END
			END

			Update [Parallel_ErpStock] set IsUpdated = 1 Where Id = @Id
		END

        FETCH NEXT FROM cursor_UpdateB2BStock INTO @Id, @Sku, @SalesOrganisationCode, @TotalOnHand, @UOM, @Weight, @WarehouseCode;
    END;

    CLOSE cursor_UpdateB2BStock;
    DEALLOCATE cursor_UpdateB2BStock;

END;