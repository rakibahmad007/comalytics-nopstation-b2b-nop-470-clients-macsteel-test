IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND OBJECT_ID = OBJECT_ID('[dbo].[ErpPercentageOfStock]'))
    EXEC('Create Function [dbo].[ErpPercentageOfStock](
	            @ErpAccountId				int = 0,
	            @AccountLevelStockAllowed	decimal(18, 4) = 0,
	            --@ProductLevelStockAllowed	decimal(18, 4) = 0,
	            --@PercentageOfAllocatedStockResetTimeUtc datetime2 = NULL,
	            @ProductId					int = 0,
	            @StockQuantity				int = 0
            )
            Returns int
            As 
            Begin
	            IF @ErpAccountId < 1 OR @ProductId < 1 OR @StockQuantity = 0 OR @AccountLevelStockAllowed = 0  -- no need to calculate (it will be always 0)
	            BEGIN
		            RETURN @StockQuantity
	            END
	            ELSE IF @AccountLevelStockAllowed >= 100  -- if Percentage Of Stock Allowed is more than or equar 100 then no need to calculate it will always same stock
	            BEGIN
		            RETURN @StockQuantity
	            END
	            ELSE
	            BEGIN
		            DECLARE @ProductLevelStockAllowed decimal(18, 4) = 0, @PercentageOfAllocatedStockResetTimeUtc datetime2 = NULL;

		            DECLARE @PercentageToConsider decimal(18, 4) = @AccountLevelStockAllowed;

		            Select top (1) @ProductLevelStockAllowed = PercentageOfAllocatedStock, 
		            @PercentageOfAllocatedStockResetTimeUtc = PercentageOfAllocatedStockResetTimeUtc 
		            From [dbo].[Erp_Special_Price] WITH(NOLOCK)
		            Where [Erp_Special_Price].[NopProductId] = @ProductId 
		            and [Erp_Special_Price].[ErpAccountId] = @ErpAccountId
		            and [Erp_Special_Price].[PercentageOfAllocatedStockResetTimeUtc] is not null 

		            If @PercentageOfAllocatedStockResetTimeUtc IS NOT NULL AND @PercentageOfAllocatedStockResetTimeUtc >= getutcdate()
		            BEGIN
			            SET @PercentageToConsider = @ProductLevelStockAllowed;
		            END

		            -- now we got per account product level
		            IF @StockQuantity > 100 AND @PercentageToConsider >= 1  -- if the stock is more than 100 and percentage is more than 1 then stock will be always more than 0
		            BEGIN
			            RETURN @StockQuantity
		            END
		            ELSE IF @StockQuantity > 10 AND @PercentageToConsider >= @StockQuantity -- if Stock Quantity is more than 10 and percentage is more then it will be always more than 0
		            BEGIN
			            RETURN @StockQuantity
		            END
		            ELSE
		            BEGIN
			            RETURN CAST(@StockQuantity * CAST(@PercentageToConsider as int) / 100 as int);
		            END
	            END
	            RETURN 0
            End'
        )
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND OBJECT_ID = OBJECT_ID('[dbo].[ErpProductPrice]'))
    EXEC('CREATE FUNCTION [dbo].[ErpProductPrice]
        (
	        @ErpAccountId			INT = 0,
	        @UsePriceGroupPricing	BIT = 0,
	        @PriceGroupCodeId		INT = 0,
	        @ProductId				INT = 0
        )
        RETURNS DECIMAL(18,2)
        AS 
            BEGIN
                DECLARE @price  DECIMAL(18,2) = 0
                IF @ErpAccountId = 0
                BEGIN
	                SELECT TOP (1) @price = p.Price 
                    FROM dbo.Product p 
                    WHERE p.Id = @ProductId
                END
                ELSE
                BEGIN 
	                IF @UsePriceGroupPricing = 0
	                BEGIN
		                SELECT TOP (1) @price = esp.Price 
                        FROM dbo.Erp_Special_Price esp
		                WHERE esp.ErpAccountId = @ErpAccountId 
                        AND esp.NopProductId = @ProductId
	                END
	                ELSE
	                BEGIN
		                SELECT TOP (1) @price = egp.Price 
                        FROM dbo.Erp_Group_Price egp
		                WHERE egp.NopProductId = @ProductId 
                        AND egp.ErpNopGroupPriceCodeId = @PriceGroupCodeId;
	                END
                END
                RETURN @price;
            END'
    )
GO


IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND OBJECT_ID = OBJECT_ID('[dbo].[seven_spikes_ajax_filters_product_sorting]'))
    EXEC('ALTER FUNCTION [dbo].[seven_spikes_ajax_filters_product_sorting] 
        (
            @OrderBy  INT, 
            @CategoryIdsCount INT, 
            @ManufacturerId INT, 
            @ParentGroupedProductId INT, 
            @UsePriceGroupPricing BIT
        )  
        RETURNS VARCHAR(250)  
    AS 
    BEGIN      
        DECLARE @sql_orderby VARCHAR(250) = ''''
        IF @OrderBy = 5   		
            SET @sql_orderby = '' p.[Name] ASC''
        ELSE IF @OrderBy = 6   		
            SET @sql_orderby = '' p.[Name] DESC''
        ELSE IF @OrderBy = 10  
            BEGIN 
                IF @UsePriceGroupPricing = 0		
                    SET @sql_orderby = '' b2bpp.[Price] ASC''
                ELSE 
                    SET @sql_orderby = '' b2bpp.[Price] ASC''
            END 	
        ELSE IF @OrderBy = 11   		
            BEGIN 
                IF @UsePriceGroupPricing = 0		
                    SET @sql_orderby = '' b2bpp.[Price] DESC'' 
                ELSE 
                    SET @sql_orderby = '' b2bpp.[Price] DESC''
            END  	
        ELSE IF @OrderBy = 15   		
            SET @sql_orderby = '' p.[CreatedOnUtc] DESC''  	
        ELSE   	
            BEGIN  		 		
                IF @CategoryIdsCount > 0 
                    SET @sql_orderby = '' pcm.DisplayOrder ASC'' 		  		 		
                IF @ManufacturerId > 0  		
                    BEGIN  			
                        IF LEN(@sql_orderby) > 0 
                            SET @sql_orderby = @sql_orderby + '', '' 			
                        SET @sql_orderby = @sql_orderby + '' pmm.DisplayOrder ASC''
                    END  		  		 		
                IF @ParentGroupedProductId > 0  		
                    BEGIN  			
                        IF LEN(@sql_orderby) > 0 
                            SET @sql_orderby = @sql_orderby + '', '' 			
                        SET @sql_orderby = @sql_orderby + '' p.[DisplayOrder] ASC'' 		
                    END  		  		 		
                IF LEN(@sql_orderby) > 0 
                    SET @sql_orderby = @sql_orderby + '', '' 		
                SET @sql_orderby = @sql_orderby + '' p.[Name] ASC''  	
            END
        RETURN @sql_orderby  
    END'
    )
GO

