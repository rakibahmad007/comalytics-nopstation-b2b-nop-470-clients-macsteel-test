CREATE PROCEDURE [dbo].[UpdateErp_Special_PriceFromParallelTableParallel_ErpAccountPricing]
AS
BEGIN
    DECLARE @Id int,
			@ListPrice decimal(18, 2),
            @Price decimal(18, 2),
            @DiscountPerc decimal(18, 4),
            @PricingNote nvarchar(max),
			@SaleOrgCode nvarchar(max),
			@AccountNumber nvarchar(max),
			@Sku nvarchar(max),
            @ErpAccountId int,
            @ProductId int,
            @SalesOrganisationId int;

    DECLARE cursor_UpdateErp_Special_Price CURSOR FOR
    SELECT
		e.[Id],
        e.[ListPrice],
        e.[Price],
        e.[DiscountPerc],
        e.[PricingNotes],
		e.[SalesOrganisationCode],
		e.[AccountNumber],
		e.[Sku]
    FROM [Parallel_ErpAccountPricing] e WITH (NOLOCK)
    WHERE e.[IsUpdated] = 0;

    OPEN cursor_UpdateErp_Special_Price;
    FETCH NEXT FROM cursor_UpdateErp_Special_Price INTO 
        @Id, @ListPrice, @Price, @DiscountPerc, @PricingNote, @SaleOrgCode, @AccountNumber, @Sku;

    WHILE @@FETCH_STATUS = 0
    BEGIN
		
		SET @SalesOrganisationId = (Select TOP 1 [Id] from [Erp_Sales_Org] where Code = @SaleOrgCode and IsActive = 1 and IsDeleted = 0)
		SET @ErpAccountId = (Select TOP 1 [Id] from Erp_Account Where AccountNumber = @AccountNumber and ErpSalesOrgId = @SalesOrganisationId and IsActive = 1 and IsDeleted = 0)
		SET @ProductId = (Select TOP 1 [Id] from Product Where Sku = @Sku and Published = 1 and Deleted = 0)

        IF EXISTS (
            SELECT 1 
            FROM [Erp_Special_Price] b
            WHERE b.[NopProductId] = @ProductId
                AND b.[ErpAccountId] = @ErpAccountId
        )
        BEGIN
            -- Update existing record
            UPDATE [Erp_Special_Price]
            SET [ListPrice] = @ListPrice,
                [Price] = @Price,
                [DiscountPerc] = @DiscountPerc,
                [PricingNote] = @PricingNote,
                [Comment] = CONCAT('Updated from Webhook - ', GETUTCDATE())
            WHERE [NopProductId] = @ProductId
                AND [ErpAccountId] = @ErpAccountId;
			Update [dbo].[Parallel_ErpAccountPricing] set IsUpdated = 1 where Id = @Id
        END
        ELSE
        BEGIN
			IF @ErpAccountId != null Or @SalesOrganisationId != null Or @ProductId != null
			BEGIN
				-- Insert new record
				INSERT INTO [Erp_Special_Price] (
				    [ErpAccountId],
				    [NopProductId],
				    [ListPrice],
				    [Price],
				    [DiscountPerc],
				    [PricingNote],
				    [Comment]
				)
				VALUES (
				    @ErpAccountId,
				    @ProductId,
				    @ListPrice,
				    @Price,
				    @DiscountPerc,
				    @PricingNote,
				    CONCAT('Inserted from Webhook - ', GETUTCDATE())
				);

				Update [dbo].[Parallel_ErpAccountPricing] set IsUpdated = 1 where Id = @Id
			END
        END

        FETCH NEXT FROM cursor_UpdateErp_Special_Price INTO 
           @Id, @ListPrice, @Price, @DiscountPerc, @PricingNote, @SaleOrgCode, @AccountNumber, @Sku;
    END;

    CLOSE cursor_UpdateErp_Special_Price;
    DEALLOCATE cursor_UpdateErp_Special_Price;

END;