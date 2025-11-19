
/****** Object:  UserDefinedFunction [dbo].[ErpPercentageOfStock]    Script Date: 16-Nov-21 1:17:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Function [dbo].[ErpPercentageOfStock](
	@B2BAccountId				int = 0,
	@AccountLevelStockAllowed	decimal(18, 4) = 0,
	@ProductId					int = 0,
	@StockQuantity				int = 0
)
Returns int
As 
Begin
	IF @B2BAccountId < 1 OR @StockQuantity = 0 OR @AccountLevelStockAllowed = 0  -- no need to calculate (it will be always 0)
	BEGIN
		RETURN @StockQuantity
	END
	ELSE IF @AccountLevelStockAllowed >= 100  -- if Percentage Of Stock Allowed is more than or equar 100 then no need to calculate it will always same stock
	BEGIN
		RETURN @StockQuantity
	END
	ELSE
	BEGIN
		DECLARE @ProductLevelStockAllowed decimal(18, 4) = 0, @PercentageOfAllocatedStockUpdatedOnUtc datetime = NULL;

		Select top (1) @ProductLevelStockAllowed = PercentageOfAllocatedStock, @PercentageOfAllocatedStockUpdatedOnUtc = PercentageOfAllocatedStockUpdatedOnUtc 
		From [dbo].[B2BPerAccountProductPricing] WITH(NOLOCK)
		Where [B2BPerAccountProductPricing].[B2BAccountId] = @B2BAccountId and [B2BPerAccountProductPricing].[ProductId] = @ProductId

		DECLARE @PercentageToConsider decimal(18, 4) = @AccountLevelStockAllowed;

		If @PercentageOfAllocatedStockUpdatedOnUtc IS NOT NULL AND day (@PercentageOfAllocatedStockUpdatedOnUtc) = day (getutcdate())
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
End;