
/****** Object:  StoredProcedure [dbo].[ErpProductLoadAllPagedNopAjaxFilters]    Script Date: 16-Nov-21 1:18:47 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[ErpProductLoadAllPagedNopAjaxFilters]
	(
		@CategoryIds nvarchar(MAX) = null,
		@ManufacturerId int = 0,
		@StoreId int = 0, 
		@VendorId int = 0, 
		@ParentGroupedProductId int = 0, 
		@ProductTypeId int = null, 
		@VisibleIndividuallyOnly bit = 0, 
		@ProductTagId int = 0, 
		@FeaturedProducts bit = null,  
		@PriceMin decimal(18, 4) = null, 
		@PriceMax decimal(18, 4) = null, 
		@Keywords nvarchar(4000) = null, 
		@SearchDescriptions bit = 0, 
		@SearchManufacturerPartNumber bit = 0, 
		@SearchSku bit = 0, 
		@SearchProductTags bit = 0, 
		@UseFullTextSearch bit = 0, 
		@FullTextMode int = 0, 
		@FilteredSpecs nvarchar(MAX) = null,  
		@FilteredProductVariantAttributes nvarchar(MAX) = null, 
		@FilteredManufacturers nvarchar(MAX) = null, 
		@FilteredVendors nvarchar(MAX) = null, 
		@OnSale bit = 0, 
		@InStock bit = 0, 
		@LanguageId int = 0, 
		@OrderBy int = 0, 
		@AllowedCustomerRoleIds nvarchar(MAX) = null,  
		@PageIndex int = 0, 
		@PageSize int = 2147483644, 
		@ShowHidden bit = 0, 
		@LoadAvailableFilters bit = 0,
		@IsErpAccount		bit = 0,
		@ErpAccountId		int = 0,
		@ErpSalesOrg_Id		int = 0,
		@UsePriceGroupPricing bit = 0,
		@PriceGroupCodeId		int = 0,
		@PreFilterFacetSpecIds nvarchar(MAX) = null, 
		@FilterableSpecificationAttributeOptionIds nvarchar(MAX) = null OUTPUT, 
		@FilterableProductVariantAttributeIds nvarchar(MAX) = null OUTPUT, 
		@FilterableManufacturerIds nvarchar(MAX) = null OUTPUT, 
		@FilterableVendorIds nvarchar(MAX) = null OUTPUT, 
		@IsOnSaleFilterEnabled bit = 0, 
		@IsInStockFilterEnabled bit = 0, 
		@HasProductsOnSale bit = 0 OUTPUT, 
		@HasProductsInStock bit = 0 OUTPUT, 
		@TotalRecords int = null OUTPUT
	)
AS  
BEGIN 
	CREATE TABLE #KeywordProducts ([ProductId] int NOT NULL) 
	
	DECLARE @SearchKeywords bit,  
	@OriginalKeywords nvarchar(4000),  
	@sql nvarchar(max),  
	@sqlWithoutFilters nvarchar(max),  
	@sql_orderby nvarchar(max),
	@PercentageOfStockAllowed int

	SET NOCOUNT ON

	--filter by Erp facet filter
	IF @IsErpAccount = 1
	BEGIN
		SET @PreFilterFacetSpecIds = isnull(@PreFilterFacetSpecIds, '')	
		CREATE TABLE #FacetFilterSpecIds
		(
			SpecificationAttributeOptionId int not null
		)
		INSERT INTO #FacetFilterSpecIds (SpecificationAttributeOptionId)
		SELECT data FROM [nop_splitstring_to_table](@PreFilterFacetSpecIds, ',')

		CREATE TABLE #ErpProductIdsByFacetFilter
		(
			Id int not null
		)
		INSERT INTO #ErpProductIdsByFacetFilter(Id)
		Select p.Id from [dbo].[Product] p
		inner join [dbo].[Product_SpecificationAttribute_Mapping] psam on psam.ProductId = p.Id 
		inner join #FacetFilterSpecIds facet on facet.SpecificationAttributeOptionId = psam.SpecificationAttributeOptionId

		Set @PercentageOfStockAllowed = (Select PercentageOfStockAllowed From [dbo].[Erp_Account] Where Id = @ErpAccountId)
	END

	SET @Keywords = isnull(@Keywords, '')  
	SET @Keywords = rtrim(ltrim(@Keywords))  
	SET @OriginalKeywords = @Keywords  
	IF ISNULL(@Keywords, '') != ''  
	BEGIN  
		SET @SearchKeywords = 1 
		IF @UseFullTextSearch = 1  
		BEGIN   
			SET @Keywords = REPLACE(@Keywords, '''', '')  
			SET @Keywords = REPLACE(@Keywords, '\"', '') 
			
			IF @FullTextMode = 0   
			BEGIN   
				SET @Keywords = ' \"' + @Keywords + '*\" '  
			END  
			ELSE  
			BEGIN   
				WHILE CHARINDEX('  ', @Keywords) > 0   
					SET @Keywords = REPLACE(@Keywords, '  ', ' ') 
		
			DECLARE @concat_term nvarchar(100)  
			
			IF @FullTextMode = 5  
			BEGIN  
				SET @concat_term = 'OR'  
			END 
				  
			IF @FullTextMode = 10  
			BEGIN  
				SET @concat_term = 'AND'  
			END 

			declare @fulltext_keywords nvarchar(4000)  
			set @fulltext_keywords = N''  
		
			declare @index int 
			set @index = CHARINDEX(' ', @Keywords, 0) 
			IF(@index = 0)  
				set @fulltext_keywords = ' \"' + @Keywords + '*\" '  
			ELSE  
			BEGIN  
				DECLARE @first BIT  
				SET  @first = 1  
				WHILE @index > 0  
				BEGIN  
					IF (@first = 0)  
						SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' '  
					ELSE
						SET @first = 0 
						SET @fulltext_keywords = @fulltext_keywords + '\"' + SUBSTRING(@Keywords, 1, @index - 1) + '*\"'  
						SET @Keywords = SUBSTRING(@Keywords, @index + 1, LEN(@Keywords) - @index)  
						SET @index = CHARINDEX(' ', @Keywords, 0)  
				end
		 
				IF LEN(@fulltext_keywords) > 0  
					SET @fulltext_keywords = @fulltext_keywords + ' ' + @concat_term + ' ' + '\"' + SUBSTRING(@Keywords, 1, LEN(@Keywords)) + '*\"'  
				END  
				SET @Keywords = @fulltext_keywords  
			END  
		END  
		ELSE  
		BEGIN   
			SET @Keywords = '%' + @Keywords + '%'  
		END 
		 
		SET @sql = '  INSERT INTO #KeywordProducts ([ProductId])  SELECT p.Id  FROM Product p with (NOLOCK)  WHERE '  
		
		IF @UseFullTextSearch = 1  
			SET @sql = @sql + 'CONTAINS(p.[Name], @Keywords) '  
		ELSE  
			SET @sql = @sql + 'PATINDEX(@Keywords, p.[Name]) > 0 '   
		
		SET @sql = @sql + '  UNION  SELECT lp.EntityId  FROM LocalizedProperty lp with (NOLOCK)  WHERE  lp.LocaleKeyGroup = N''Product''  AND lp.LanguageId = ' 
		+ ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '  AND lp.LocaleKey = N''Name'''  
	
		IF @UseFullTextSearch = 1  
			SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '  
		ELSE  
			SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '  

		IF @SearchDescriptions = 1  
		BEGIN   
			SET @sql = @sql + '  UNION  SELECT p.Id  FROM Product p with (NOLOCK)  WHERE '  
		
			IF @UseFullTextSearch = 1  
				SET @sql = @sql + 'CONTAINS(p.[ShortDescription], @Keywords) '  
			ELSE  
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[ShortDescription]) > 0 '   
		
			SET @sql = @sql + '  UNION  SELECT p.Id  FROM Product p with (NOLOCK)  WHERE '  
		
			IF @UseFullTextSearch = 1  
				SET @sql = @sql + 'CONTAINS(p.[FullDescription], @Keywords) '  
			ELSE  
				SET @sql = @sql + 'PATINDEX(@Keywords, p.[FullDescription]) > 0 ' 
		
			SET @sql = @sql + '  UNION  SELECT lp.EntityId  FROM LocalizedProperty lp with (NOLOCK)  WHERE  lp.LocaleKeyGroup = N''Product''  AND lp.LanguageId = ' 
			+ ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '  AND lp.LocaleKey = N''ShortDescription'''  
	
			IF @UseFullTextSearch = 1  
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '  
			ELSE  
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '   

			SET @sql = @sql + '  UNION  SELECT lp.EntityId  FROM LocalizedProperty lp with (NOLOCK)  WHERE  lp.LocaleKeyGroup = N''Product''  AND lp.LanguageId = ' 
			+ ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '  AND lp.LocaleKey = N''FullDescription'''  
		
			IF @UseFullTextSearch = 1  
				SET @sql = @sql + ' AND CONTAINS(lp.[LocaleValue], @Keywords) '  
			ELSE  
				SET @sql = @sql + ' AND PATINDEX(@Keywords, lp.[LocaleValue]) > 0 '  
		END
	 
		IF @SearchManufacturerPartNumber = 1  
		BEGIN  
			SET @sql = @sql + '  UNION  SELECT p.Id  FROM Product p with (NOLOCK)  WHERE p.[ManufacturerPartNumber] = @OriginalKeywords '  
		END 
	
		IF @SearchSku = 1  
		BEGIN  
			SET @sql = @sql + '  UNION  SELECT p.Id  FROM Product p with (NOLOCK)  WHERE p.[Sku] = @OriginalKeywords '  
		END 
	
		IF @SearchProductTags = 1  
		BEGIN   
			SET @sql = @sql + '  UNION  SELECT pptm.Product_Id  FROM Product_ProductTag_Mapping pptm with(NOLOCK) INNER JOIN ProductTag pt with(NOLOCK) ON pt.Id = pptm.ProductTag_Id  WHERE pt.[Name] = @OriginalKeywords ' 
			SET @sql = @sql + '  UNION  SELECT pptm.Product_Id  FROM LocalizedProperty lp with (NOLOCK) 
			INNER JOIN Product_ProductTag_Mapping pptm with(NOLOCK) ON lp.EntityId = pptm.ProductTag_Id  WHERE  lp.LocaleKeyGroup = N''ProductTag''  AND lp.LanguageId = ' 
			+ ISNULL(CAST(@LanguageId AS nvarchar(max)), '0') + '  AND lp.LocaleKey = N''Name''  AND lp.[LocaleValue] = @OriginalKeywords '  
		END 

		EXEC sp_executesql @sql, N'@Keywords nvarchar(4000), @OriginalKeywords nvarchar(4000)', @Keywords, @OriginalKeywords 
	END  
	ELSE  
	BEGIN  
		SET @SearchKeywords = 0  
	END 

	SET @CategoryIds = isnull(@CategoryIds, '')  
	
	CREATE TABLE #FilteredCategoryIds  (  CategoryId int not null  )  INSERT INTO #FilteredCategoryIds (CategoryId)  
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@CategoryIds, ',')  
	
	DECLARE @CategoryIdsCount int  
	SET @CategoryIdsCount = (SELECT COUNT(1) FROM #FilteredCategoryIds) SET @FilteredSpecs = isnull(@FilteredSpecs, '')  
	
	CREATE TABLE #FilteredSpecificationAttributeOptions  (  SpecificationAttributeOptionId int not null unique  ) 
	INSERT INTO #FilteredSpecificationAttributeOptions (SpecificationAttributeOptionId)  
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@FilteredSpecs, ',') 
	
	DECLARE @SpecificationAttributesCount int  
	SET @SpecificationAttributesCount =   (  SELECT COUNT(DISTINCT sao.SpecificationAttributeId) 
	FROM #FilteredSpecificationAttributeOptions fs   INNER JOIN SpecificationAttributeOption sao ON sao.Id = fs.SpecificationAttributeOptionId   ) 
	
	CREATE TABLE #FilteredSpecificationAttributes  (  AttributeId int not null  ) 
	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredSpecificationAttributes_AttributeId  ON #FilteredSpecificationAttributes (AttributeId); 
	
	INSERT INTO #FilteredSpecificationAttributes  
	SELECT DISTINCT sap.SpecificationAttributeId  FROM SpecificationAttributeOption sap  
	INNER JOIN #FilteredSpecificationAttributeOptions fs ON fs.SpecificationAttributeOptionId = sap.Id 
	
	SET @FilteredProductVariantAttributes = isnull(@FilteredProductVariantAttributes, '')  
	
	CREATE TABLE #FilteredProductVariantAttributes  (  ProductVariantAttributeId int not null  ) 
	CREATE INDEX IX_FilteredProductVariantAttributes_ProductVariantAttributeId  ON #FilteredProductVariantAttributes (ProductVariantAttributeId); 

	INSERT INTO #FilteredProductVariantAttributes (ProductVariantAttributeId)  
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@FilteredProductVariantAttributes, ',') 
	
	DECLARE @ProductAttributesCount int  
	SET @ProductAttributesCount =   (  SELECT COUNT(DISTINCT ppm.ProductAttributeId) 
	FROM #FilteredProductVariantAttributes fpva   INNER JOIN Product_ProductAttribute_Mapping ppm ON ppm.Id = fpva.ProductVariantAttributeId   ) 
	
	CREATE TABLE #FilteredProductAttributes  (  AttributeId int not null  ) 
	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredAttributes_AttributeId  ON #FilteredProductAttributes (AttributeId); 
	
	INSERT INTO #FilteredProductAttributes  SELECT DISTINCT ProductAttributeId  
	FROM Product_ProductAttribute_Mapping ppm  INNER JOIN #FilteredProductVariantAttributes fpv ON fpv.ProductVariantAttributeId = ppm.Id 
	
	SET @FilteredManufacturers = isnull(@FilteredManufacturers, '')  

	CREATE TABLE #FilteredManufacturers  (  ManufacturerId int not null  )  
	INSERT INTO #FilteredManufacturers (ManufacturerId)  
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@FilteredManufacturers, ',')  
	
	DECLARE @ManufacturersCount int  
	SET @ManufacturersCount = (SELECT COUNT(1) FROM #FilteredManufacturers) 
	
	SET @FilteredVendors = isnull(@FilteredVendors, '')  
	CREATE TABLE #FilteredVendorIds  (  VendorId int not null  )  
	INSERT INTO #FilteredVendorIds (VendorId)  SELECT CAST(data as int) FROM [nop_splitstring_to_table](@FilteredVendors, ',') 

	SET @AllowedCustomerRoleIds = isnull(@AllowedCustomerRoleIds, '')  
	
	CREATE TABLE #FilteredCustomerRoleIds  (  CustomerRoleId int not null  )  
	INSERT INTO #FilteredCustomerRoleIds (CustomerRoleId)  
	SELECT CAST(data as int) FROM [nop_splitstring_to_table](@AllowedCustomerRoleIds, ',')  
	
	DECLARE @VendorsCount int  
	SET @VendorsCount = (SELECT COUNT(1) FROM #FilteredVendorIds) 

	DECLARE @PageLowerBound int  
	DECLARE @PageUpperBound int  
	DECLARE @RowsToReturn int  
	
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)  
	SET @PageLowerBound = @PageSize * @PageIndex  
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1 

	CREATE TABLE #DisplayOrderTmp   (  [Id] int IDENTITY (1, 1) NOT NULL,  [ProductId] int NOT NULL,  [ChildProductId] int  )  
	SET @sql = '  INSERT INTO #DisplayOrderTmp ([ProductId], [ChildProductId])  SELECT p.Id, ISNULL(cp.Id, 0)  FROM  Product p with (NOLOCK)  LEFT JOIN Product cp with (NOLOCK)  ON p.Id = cp.ParentGroupedProductId' 
	
	IF @CategoryIdsCount > 0  
	BEGIN  
	SET @sql = @sql + '  LEFT JOIN Product_Category_Mapping pcm with (NOLOCK)  ON p.Id = pcm.ProductId'  
	END 
	
	IF @ManufacturerId > 0 OR @ManufacturersCount > 0  
	BEGIN  
	SET @sql = @sql + '  LEFT JOIN Product_Manufacturer_Mapping pmm with (NOLOCK)  ON p.Id = pmm.ProductId'  
	END
	if @OrderBy = 10 or @OrderBy = 11 
	BEGIN
		If @UsePriceGroupPricing = 0 
		BEGIN  
			SET @sql = @sql + '  LEFT JOIN ErpPerAccountProductPricing erppp with (NOLOCK)  ON p.Id = erppp.ProductId'  
		END 
		else
		BEGIN  
			SET @sql = @sql + '  LEFT JOIN ErpPriceGroupProductPricing erppgp with (NOLOCK)  ON p.Id = erppgp.ProductId'  
		END 
	END 
	IF ISNULL(@ProductTagId, 0) != 0  
	BEGIN  
	SET @sql = @sql + '  LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK)  ON p.Id = pptm.Product_Id'  
	END 
	
	IF @SearchKeywords = 1  
	BEGIN  
	SET @sql = @sql + '  JOIN #KeywordProducts kp  ON  p.Id = kp.ProductId'  
	END 
	
	SET @sql = @sql + '  WHERE  p.Deleted = 0' 
	SET @sql = @sql + '  AND  (p.ParentGroupedProductId = 0 OR p.VisibleIndividually = 1)' 

	if @OrderBy = 10 or @OrderBy = 11 
	BEGIN
		If @UsePriceGroupPricing = 0 
		BEGIN  
			SET @sql = @sql + '  AND  erppp.ErpAccountId = ' + CAST(@ErpAccountId AS nvarchar(max))  
		END 
		else
		BEGIN  
			SET @sql = @sql + '  AND  erppgp.ErpPriceGroupCodeId '  + CAST(@PriceGroupCodeId AS nvarchar(max))  
		END 
	END 

	IF @CategoryIdsCount > 0  
	BEGIN  
		SET @sql = @sql + '  AND pcm.CategoryId IN (SELECT CategoryId FROM #FilteredCategoryIds)' 

		IF @FeaturedProducts IS NOT NULL  
		BEGIN  
		SET @sql = @sql + '  AND pcm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))  
		END
	END 

	IF @ManufacturerId > 0  
	BEGIN  
		SET @sql = @sql + '  AND pmm.ManufacturerId = ' + CAST(@ManufacturerId AS nvarchar(max)) 
		
		IF @FeaturedProducts IS NOT NULL  
		BEGIN  
			SET @sql = @sql + '  AND pmm.IsFeaturedProduct = ' + CAST(@FeaturedProducts AS nvarchar(max))  
		END  
	END
	 
	IF @VendorId > 0  
	BEGIN  
		SET @sql = @sql + '  AND p.VendorId = ' + CAST(@VendorId AS nvarchar(max))  
	END 

	IF @ParentGroupedProductId > 0  
	BEGIN  
		SET @sql = @sql + '  AND p.ParentGroupedProductId = ' + CAST(@ParentGroupedProductId AS nvarchar(max))  
	END 

	IF @OnSale = 1  
	BEGIN  
		SET @sql = @sql + '  AND   (  (cp.ID IS NULL AND p.OldPrice > 0  AND p.OldPrice != p.Price) OR (cp.ID IS NOT NULL AND cp.OldPrice > 0  AND cp.OldPrice != cp.Price)  )'  
	END   

	--IF @InStock = 1  
	--BEGIN
	--	SET @sql = @sql + '  AND   (  (cp.ID IS NULL  AND   (  (p.ManageInventoryMethodId = 0) OR  (P.ManageInventoryMethodId = 1 AND  (  (p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) OR   
	--	(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = p.Id'

	--	-- for erp stock check by sales org specific warehouses
	--	IF @IsErpAccount = 1
	--	BEGIN
	--		SET @sql = @sql + ' AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = ' + CAST(@ErpSalesOrg_Id AS nvarchar(max))+')' 
	--	END

	--	SET @sql = @sql + ' AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND p.UseMultipleWarehouses = 1)  )  )  )  )  OR 
	--	(p.Id IS NOT NULL AND   (  (cp.ManageInventoryMethodId = 0) OR  (cp.ManageInventoryMethodId = 1 AND  (  (cp.StockQuantity > 0 AND cp.UseMultipleWarehouses = 0) OR   
	--	(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = cp.Id ' 

	--	-- for Erp stock check by sales org specific warehouses
	--	IF @IsErpAccount = 1
	--	BEGIN
	--		SET @sql = @sql + ' AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = ' + CAST(@ErpSalesOrg_Id AS nvarchar(max))+')' 
	--	END

	--	SET @sql = @sql + '  AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND cp.UseMultipleWarehouses = 1)  )  )  )  )  )' 
	--END

	IF @InStock = 1  
	BEGIN
		SET @sql = @sql + '  AND   (  (cp.ID IS NULL  AND   (  (p.ManageInventoryMethodId = 0) OR  (P.ManageInventoryMethodId = 1 AND  (  (p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) OR   
		(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = p.Id'

		-- for erp stock check by sales org specific warehouses
		IF @IsErpAccount = 1 AND @PercentageOfStockAllowed < 100
		BEGIN
			SET @sql = @sql + ' AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = ' + CAST(@ErpSalesOrg_Id AS nvarchar(max))+')
			AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity AND [dbo].[ErpPercentageOfStock](' + CAST(@ErpAccountId AS nvarchar(max)) + ',' + CAST(@PercentageOfStockAllowed AS nvarchar(max)) + ', [pwi].ProductId, [pwi].StockQuantity) > 0)'
		END
		ELSE IF @IsErpAccount = 1
		BEGIN
			SET @sql = @sql + ' AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = ' + CAST(@ErpSalesOrg_Id AS nvarchar(max))+')
			AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity)'
		END
		ELSE
		BEGIN
			SET @sql = @sql + ' AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity)' 
		END

		SET @sql = @sql + ' AND p.UseMultipleWarehouses = 1)  )  )  )  )  OR 
			(p.Id IS NOT NULL AND   (  (cp.ManageInventoryMethodId = 0) OR  (cp.ManageInventoryMethodId = 1 AND  (  (cp.StockQuantity > 0 AND cp.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = cp.Id' 

		-- for erp stock check by sales org specific warehouses
		IF @IsErpAccount = 1 AND @PercentageOfStockAllowed < 100
		BEGIN
			SET @sql = @sql + ' AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = ' + CAST(@ErpSalesOrg_Id AS nvarchar(max))+')
			AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity AND [dbo].[ErpPercentageOfStock](' + CAST(@ErpAccountId AS nvarchar(max)) + ',' + CAST(@PercentageOfStockAllowed AS nvarchar(max)) + ', [pwi].ProductId, [pwi].StockQuantity) > 0)' 		 
		END
		ELSE IF @IsErpAccount = 1
		BEGIN
			SET @sql = @sql + ' AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = ' + CAST(@ErpSalesOrg_Id AS nvarchar(max))+')
			AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity)' 		 
		END
		ELSE
		BEGIN
			SET @sql = @sql + '  AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity)' 
		END

		SET @sql = @sql + ' AND cp.UseMultipleWarehouses = 1)  )  )  )  )  )' 
	END

	IF @ProductTypeId is not null  
	BEGIN  
		SET @sql = @sql + '  AND p.ProductTypeId = ' + CAST(@ProductTypeId AS nvarchar(max))  
	END 

	IF @VisibleIndividuallyOnly = 1  
	BEGIN  
		SET @sql = @sql + '  AND p.VisibleIndividually = 1'  
	END 

	IF ISNULL(@ProductTagId, 0) != 0  
	BEGIN  
		SET @sql = @sql + '  AND pptm.ProductTag_Id = ' + CAST(@ProductTagId AS nvarchar(max))  
	END 

	IF @ShowHidden = 0  
	BEGIN  
		SET @sql = @sql + '  AND p.Published = 1  AND p.Deleted = 0  AND (getutcdate() BETWEEN ISNULL(p.AvailableStartDateTimeUtc, ''1/1/1900'') and ISNULL(p.AvailableEndDateTimeUtc, ''1/1/2999''))'  
	END   

	IF @PriceMin > 0  
	BEGIN
		IF @IsErpAccount = 0
		BEGIN
			SET @sql = @sql + '  AND (  (  cp.Id IS NULL AND (p.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')  )  OR  (  (cp.Price >= ' + CAST(@PriceMin AS nvarchar(max)) + ')  )  )'  
		END
		ELSE
		BEGIN
			SET @sql = @sql + '  AND (  (  cp.Id IS NULL AND ([dbo].ErpProductPrice(' + CAST(@ErpAccountId AS nvarchar(max))+', ' + CAST(@UsePriceGroupPricing AS nvarchar(max))+', +
					' + CAST(@PriceGroupCodeId AS nvarchar(max))+', p.Id) >= ' + CAST(@PriceMin AS nvarchar(max)) + ')  )  
					OR  (  ([dbo].ErpProductPrice(' + CAST(@ErpAccountId AS nvarchar(max))+', ' + CAST(@UsePriceGroupPricing AS nvarchar(max))+', +
					' + CAST(@PriceGroupCodeId AS nvarchar(max))+', cp.Id) >= ' + CAST(@PriceMin AS nvarchar(max)) + ')  )  )'  
		END
	END   

	IF @PriceMax > 0  
	BEGIN
		IF @IsErpAccount = 0
		BEGIN
			SET @sql = @sql + '  AND (  (  cp.Id IS NULL AND (p.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')  )  OR  (  (cp.Price <= ' + CAST(@PriceMax AS nvarchar(max)) + ')  )  )'  
		END
		ELSE
		BEGIN
			SET @sql = @sql + '  AND (  (  cp.Id IS NULL AND ([dbo].ErpProductPrice(' + CAST(@ErpAccountId AS nvarchar(max))+', ' + CAST(@UsePriceGroupPricing AS nvarchar(max))+', +
					' + CAST(@PriceGroupCodeId AS nvarchar(max))+', p.Id) <= ' + CAST(@PriceMax AS nvarchar(max)) + ')  )  
			OR  (  ([dbo].ErpProductPrice(' + CAST(@ErpAccountId AS nvarchar(max))+', ' + CAST(@UsePriceGroupPricing AS nvarchar(max))+', +
					' + CAST(@PriceGroupCodeId AS nvarchar(max))+', cp.Id) <= ' + CAST(@PriceMax AS nvarchar(max)) + ')  )  )'  
		END  
	END   

	IF @ShowHidden = 0  
	BEGIN  
		SET @sql = @sql + '  AND (p.SubjectToAcl = 0 OR EXISTS (  SELECT 1 FROM #FilteredCustomerRoleIds [fcr]  WHERE  [fcr].CustomerRoleId IN 
		(  SELECT [acl].CustomerRoleId  FROM [AclRecord] acl with (NOLOCK)  WHERE [acl].EntityId = p.Id AND [acl].EntityName = ''Product''  )  ))'  
	END 

	IF @StoreId > 0  
	BEGIN  
		SET @sql = @sql + '  AND (p.LimitedToStores = 0 OR EXISTS (  SELECT 1 FROM [StoreMapping] sm with (NOLOCK) 
		WHERE [sm].EntityId = p.Id AND [sm].EntityName = ''Product'' and [sm].StoreId=' + CAST(@StoreId AS nvarchar(max)) + '  ))'  
	END  

	SET @sqlWithoutFilters = @sql 

	IF @SpecificationAttributesCount > 0  
	BEGIN  
		SET @sql = @sql + '  AND (  (SELECT AttributesCount FROM #FilteredSpecificationAttributesToProduct fsatp  WHERE p.Id = fsatp.ProductId) = ' + CAST(@SpecificationAttributesCount AS nvarchar(max)) +   ')'  
	END   

	IF @ProductAttributesCount > 0  
	BEGIN  
		SET @sql = @sql + '  AND (  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct fpatp  WHERE (cp.Id IS NULL AND p.Id = fpatp.ProductId) OR cp.Id = fpatp.ProductId) = ' + CAST(@ProductAttributesCount AS nvarchar(max)) +   ')'  
	END 

	IF @ManufacturersCount > 0  
	BEGIN  
		SET @sql = @sql + '  AND pmm.ManufacturerId IN (SELECT ManufacturerId FROM #FilteredManufacturers)'  
	END 

	IF @VendorsCount > 0  
	BEGIN 
		SET @sql = @sql + '   AND p.VendorId IN (SELECT VendorId FROM #FilteredVendorIds)'  
	END 

	-- erp pre filter facet
	IF @IsErpAccount = 1
	BEGIN
		SET @sql = @sql + ' AND p.Id IN (Select Id from #ErpProductIdsByFacetFilter)'
	END

	SET @sql_orderby = [dbo].[seven_spikes_ajax_filters_product_sorting] (@OrderBy, @CategoryIdsCount, @ManufacturerId, @ParentGroupedProductId, @UsePriceGroupPricing) 
	SET @sql = @sql + '  ORDER BY' + @sql_orderby   

	EXEC sp_executesql @sqlWithoutFilters 
	
	CREATE TABLE #ProductIdsBeforeFiltersApplied   (  [ProductId] int NOT NULL,  [ChildProductId] int  ) 
	CREATE UNIQUE CLUSTERED INDEX IX_ProductIds_ProductId  ON #ProductIdsBeforeFiltersApplied (ProductId, ChildProductId); 
	
	INSERT INTO #ProductIdsBeforeFiltersApplied ([ProductId], [ChildProductId])  SELECT ProductId, ChildProductId  
	FROM #DisplayOrderTmp  GROUP BY ProductId, ChildProductId  ORDER BY min([Id]) 

	DELETE FROM #DisplayOrderTmp   
	
	CREATE TABLE #FilteredSpecificationAttributesToProduct  (  ProductId int not null,  AttributesCount int not null  ) 
	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredSpecificationAttributesToProduct_ProductId  ON #FilteredSpecificationAttributesToProduct (ProductId) 
	
	IF @SpecificationAttributesCount > 0  
	BEGIN 
		IF @SpecificationAttributesCount > 1  
		BEGIN 
			INSERT INTO #FilteredSpecificationAttributesToProduct  
			SELECT psm.ProductId, COUNT (DISTINCT sao.SpecificationAttributeId)  FROM Product_SpecificationAttribute_Mapping psm  
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = psm.ProductId  INNER JOIN #FilteredSpecificationAttributeOptions fs 
			ON fs.SpecificationAttributeOptionId = psm.SpecificationAttributeOptionId  
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId  
			GROUP BY psm.ProductId  HAVING COUNT (DISTINCT sao.SpecificationAttributeId) >= @SpecificationAttributesCount - 1  
		END 

		IF @SpecificationAttributesCount = 1  
		BEGIN 
			INSERT INTO #FilteredSpecificationAttributesToProduct  
			SELECT DISTINCT psm.ProductId, 1  
			FROM Product_SpecificationAttribute_Mapping psm  
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = psm.ProductId  INNER JOIN #FilteredSpecificationAttributeOptions fs 
			ON fs.SpecificationAttributeOptionId = psm.SpecificationAttributeOptionId AND psm.AllowFiltering = 1 
			
			INSERT INTO #FilteredSpecificationAttributesToProduct  SELECT DISTINCT psm.ProductId, 0  FROM Product_SpecificationAttribute_Mapping psm  INNER JOIN #ProductIdsBeforeFiltersApplied p 
			ON p.ProductId = psm.ProductId  
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId  INNER JOIN #FilteredSpecificationAttributes fsa 
			ON fsa.AttributeId = sao.SpecificationAttributeId  
			WHERE NOT EXISTS (SELECT NULL FROM #FilteredSpecificationAttributesToProduct fsatp WHERE fsatp.ProductId = psm.ProductId) AND psm.AllowFiltering = 1 
		END   
  
		IF @SpecificationAttributesCount > 1  
		BEGIN 
			DELETE #FilteredSpecificationAttributesToProduct  FROM #FilteredSpecificationAttributesToProduct fsatp 
			WHERE (SELECT COUNT (DISTINCT sao.SpecificationAttributeId)  FROM Product_SpecificationAttribute_Mapping psm  
			INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId  INNER JOIN #FilteredSpecificationAttributes fsa ON fsa.AttributeId = sao.SpecificationAttributeId  
			WHERE psm.ProductId = fsatp.ProductId) < @SpecificationAttributesCount 
		END 
	END 
			
	CREATE TABLE #FilteredProductAttributesToProduct  (  ProductId int not null,  AttributesCount int not null  ) 
	CREATE UNIQUE CLUSTERED INDEX IX_#FilteredProductAttributesToProduct_ProductId  ON #FilteredProductAttributesToProduct (ProductId) 
	
	IF @ProductAttributesCount > 0  
	BEGIN 
		IF @ProductAttributesCount > 1  
		BEGIN 
			INSERT INTO #FilteredProductAttributesToProduct  SELECT ppm.ProductId, COUNT (DISTINCT ppm.ProductAttributeId)  FROM Product_ProductAttribute_Mapping ppm  
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = ppm.ProductId OR p.ChildProductId = ppm.ProductId  
			INNER JOIN #FilteredProductVariantAttributes fpva ON fpva.ProductVariantAttributeId = ppm.Id  
			GROUP BY ppm.ProductId  HAVING COUNT(DISTINCT ppm.ProductAttributeId) >= @ProductAttributesCount - 1 
		END 

		IF @ProductAttributesCount = 1  
		BEGIN 
			INSERT INTO #FilteredProductAttributesToProduct  
			SELECT DISTINCT ppm.ProductId, 1  FROM Product_ProductAttribute_Mapping ppm  
			INNER JOIN #ProductIdsBeforeFiltersApplied p ON p.ProductId = ppm.ProductId OR p.ChildProductId = ppm.ProductId  
			INNER JOIN #FilteredProductVariantAttributes fpva ON fpva.ProductVariantAttributeId = ppm.Id 

			INSERT INTO #FilteredProductAttributesToProduct  
			SELECT DISTINCT ppm.ProductId, 0  FROM Product_ProductAttribute_Mapping ppm  
			INNER JOIN #ProductIdsBeforeFiltersApplied p 
			ON p.ProductId = ppm.ProductId OR p.ChildProductId = ppm.ProductId  
			INNER JOIN #FilteredProductAttributes fa ON fa.AttributeId = ppm.ProductAttributeId  
			WHERE ppm.ProductId NOT IN (SELECT ProductId FROM #FilteredProductAttributesToProduct) 
		END 

		IF @ProductAttributesCount > 1  
		BEGIN 
			DELETE #FilteredProductAttributesToProduct  FROM #FilteredProductAttributesToProduct fpatp  
			WHERE (SELECT COUNT(DISTINCT ppm.ProductAttributeId) FROM  Product_ProductAttribute_Mapping ppm  INNER JOIN #FilteredProductAttributes fa 
			ON fa.AttributeId = ppm.ProductAttributeId  WHERE ppm.ProductId = fpatp.ProductId) < @ProductAttributesCount 
		END 
	END   
	
	EXEC sp_executesql @sql 
	
	CREATE TABLE #PageIndex   (  [IndexId] int IDENTITY (1, 1) NOT NULL,  [ProductId] int NOT NULL,  [ChildProductId] int  ) 
	INSERT INTO #PageIndex ([ProductId], [ChildProductId])  SELECT ProductId, ChildProductId  FROM #DisplayOrderTmp  
	GROUP BY ProductId, ChildProductId  ORDER BY min([Id]) 
	
	SET @TotalRecords = @@rowcount   
	
	IF @LoadAvailableFilters = 1  
	BEGIN 
		CREATE TABLE #PotentialProductSpecificationAttributeIds   (  [ProductId] int NOT NULL,  [SpecificationAttributeOptionId] int NOT NULL  ) 

		INSERT INTO #PotentialProductSpecificationAttributeIds ([ProductId], [SpecificationAttributeOptionId])  
		SELECT psm.ProductId, psm.SpecificationAttributeOptionId  FROM Product_SpecificationAttribute_Mapping psm  
		INNER JOIN #FilteredSpecificationAttributesToProduct fsatp on fsatp.ProductId = psm.ProductId  
		INNER JOIN SpecificationAttributeOption sao ON sao.Id = psm.SpecificationAttributeOptionId  
		INNER JOIN #FilteredSpecificationAttributes fsa ON fsa.AttributeId = sao.SpecificationAttributeId 
		WHERE fsatp.AttributesCount = @SpecificationAttributesCount - 1 AND  sao.SpecificationAttributeId NOT IN (SELECT sao.SpecificationAttributeId FROM Product_SpecificationAttribute_Mapping psm1  
		INNER JOIN SpecificationAttributeOption sao1 ON sao1.Id = psm1.SpecificationAttributeOptionId  
		INNER JOIN #FilteredSpecificationAttributeOptions fs ON fs.SpecificationAttributeOptionId = sao.Id  WHERE psm1.ProductId = psm.ProductId) 

		IF @ProductAttributesCount > 0  
		BEGIN  
			DELETE #PotentialProductSpecificationAttributeIds  FROM #PotentialProductSpecificationAttributeIds ppsa  
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = ppsa.ProductId  
			WHERE   (  pibfa.ChildProductId = 0 AND  (  NOT EXISTS (SELECT NULL FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ProductId)  OR  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct 
			WHERE ProductId = pibfa.ProductId) != @ProductAttributesCount  )  )  OR  (  pibfa.ChildProductId != 0 AND  (  NOT EXISTS (SELECT NULL FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ChildProductId)  
			OR  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ChildProductId) != @ProductAttributesCount  )  )  
		END 
	
		IF @ManufacturersCount > 0  
		BEGIN  
			DELETE FROM #PotentialProductSpecificationAttributeIds  WHERE NOT EXISTS (  SELECT NULL FROM Product_Manufacturer_Mapping [pmm]   
			INNER JOIN #FilteredManufacturers [fm] ON [fm].ManufacturerId = [pmm].ManufacturerId  WHERE [pmm].ProductId = #PotentialProductSpecificationAttributeIds.ProductId)  
		END 

		IF @VendorsCount > 0  
		BEGIN  
			DELETE FROM #PotentialProductSpecificationAttributeIds  WHERE NOT EXISTS (  SELECT NULL FROM Product [p]   
			INNER JOIN #FilteredVendorIds [fv] ON [fv].VendorId = [p].VendorId  
			WHERE [p].Id = #PotentialProductSpecificationAttributeIds.ProductId)  
		END 

		CREATE TABLE #FilterableSpecs   (  [ProductId] int NOT NULL,  [SpecificationAttributeOptionId] int NOT NULL  ) 
		CREATE TABLE #FilterableSpecsDistinct   (  [SpecificationAttributeOptionId] int NOT NULL  )  
	
		INSERT INTO #FilterableSpecs ([ProductId], [SpecificationAttributeOptionId])  
		SELECT DISTINCT [psam].ProductId, [psam].SpecificationAttributeOptionId  
		FROM [Product_SpecificationAttribute_Mapping] [psam] with (NOLOCK) 
		WHERE [psam].[ProductId] IN (SELECT [pi].ProductId FROM #PageIndex [pi]) AND [psam].[AllowFiltering] = 1 
	
		INSERT INTO #FilterableSpecs ([ProductId], [SpecificationAttributeOptionId])  
		SELECT DISTINCT ProductId, SpecificationAttributeOptionId  FROM #PotentialProductSpecificationAttributeIds 
	
		INSERT INTO #FilterableSpecsDistinct ([SpecificationAttributeOptionId])  
		SELECT DISTINCT SpecificationAttributeOptionId  FROM #FilterableSpecs 
		SELECT @FilterableSpecificationAttributeOptionIds = COALESCE(@FilterableSpecificationAttributeOptionIds + ',' , '') + CAST(SpecificationAttributeOptionId as nvarchar(4000)) FROM #FilterableSpecsDistinct   
	
		CREATE TABLE #PotentialProductVariantAttributeIds   (  [ProductId] int NOT NULL,  [ProductVariantAttributeId] int NOT NULL  ) 
		CREATE INDEX IX_PotentialProductVariantAttributeIds_ProductId  ON #PotentialProductVariantAttributeIds (ProductId);   
	
		INSERT INTO #PotentialProductVariantAttributeIds ([ProductId], [ProductVariantAttributeId])  
		SELECT [ppm].ProductId, [ppm].Id  FROM Product_ProductAttribute_Mapping [ppm]  
		INNER JOIN #FilteredProductAttributesToProduct fpatp ON fpatp.ProductId = [ppm].ProductId  
		INNER JOIN #FilteredProductAttributes fa ON fa.AttributeId = ppm.ProductAttributeId  
		WHERE fpatp.AttributesCount = @ProductAttributesCount - 1 AND   [ppm].Id NOT IN (SELECT ProductVariantAttributeId FROM #FilteredProductVariantAttributes)  
	
		IF @SpecificationAttributesCount > 0  
		BEGIN 
			DELETE #PotentialProductVariantAttributeIds  FROM #PotentialProductVariantAttributeIds ppva  
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = ppva.ProductId OR pibfa.ChildProductId = ppva.ProductId  WHERE   
			(  NOT EXISTS (SELECT NULL FROM #FilteredSpecificationAttributesToProduct WHERE ProductId = pibfa.ProductId)  OR  (SELECT AttributesCount FROM #FilteredSpecificationAttributesToProduct 
			WHERE ProductId = pibfa.ProductId) != @SpecificationAttributesCount  ) 
		END 

		IF @ManufacturersCount > 0  
		BEGIN  
			DELETE FROM #PotentialProductVariantAttributeIds  
			WHERE NOT EXISTS (  SELECT NULL FROM Product_Manufacturer_Mapping pmm  
			INNER JOIN #FilteredManufacturers fm 
			ON fm.ManufacturerId = pmm.ManufacturerId  
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = pmm.ProductId  
			WHERE #PotentialProductVariantAttributeIds.ProductId = pibfa.ProductId OR #PotentialProductVariantAttributeIds.ProductId = pibfa.ChildProductId)  
		END
	  
		IF @VendorsCount > 0  
		BEGIN  
			DELETE FROM #PotentialProductVariantAttributeIds  
			WHERE NOT EXISTS (  SELECT NULL FROM Product [p]   INNER JOIN #FilteredVendorIds [fv] ON [fv].VendorId = [p].VendorId  
			INNER JOIN #ProductIdsBeforeFiltersApplied 
			ON #PotentialProductVariantAttributeIds.ProductId = #ProductIdsBeforeFiltersApplied.ProductId  
			OR #PotentialProductVariantAttributeIds.ProductId = #ProductIdsBeforeFiltersApplied.ChildProductId  
			WHERE [p].Id = #ProductIdsBeforeFiltersApplied.ProductId OR [p].Id = #ProductIdsBeforeFiltersApplied.ChildProductId)  
		END 

		CREATE TABLE #FilterableProductVariantIds   (  [ProductId] int NOT NULL,  [ProductVariantAttributeId] int NOT NULL  ) 
		CREATE TABLE #FilterableProductVariantIdsDistinct   (  [ProductVariantAttributeId] int NOT NULL  )  

		INSERT INTO #FilterableProductVariantIds ([ProductId], [ProductVariantAttributeId])  SELECT DISTINCT [ppm].ProductId, [ppm].Id  
		FROM [Product_ProductAttribute_Mapping] [ppm]  INNER JOIN #PageIndex [pi] 
		ON [pi].ProductId = [ppm].[ProductId] OR [pi].ChildProductId = [ppm].ProductId 
	
		INSERT INTO #FilterableProductVariantIds ([ProductId], [ProductVariantAttributeId])  
		SELECT DISTINCT ProductId, ProductVariantAttributeId  FROM #PotentialProductVariantAttributeIds 

		INSERT INTO #FilterableProductVariantIdsDistinct ([ProductVariantAttributeId])  
		SELECT DISTINCT ProductVariantAttributeId  FROM #FilterableProductVariantIds 
		SELECT @FilterableProductVariantAttributeIds = COALESCE(@FilterableProductVariantAttributeIds + ',' , '') + CAST(ProductVariantAttributeId as nvarchar(4000))  FROM #FilterableProductVariantIdsDistinct   

		CREATE TABLE #FilterableManufacturers   (  [ProductId] int NOT NULL,  [ManufacturerId] int NOT NULL  ) 
		CREATE TABLE #FilterableManufacturersDistinct   (  [ManufacturerId] int NOT NULL  )  

		INSERT INTO #FilterableManufacturers ([ProductId], [ManufacturerId])  
		SELECT DISTINCT [pmm].ProductId, [pmm].ManufacturerId  FROM Product_Manufacturer_Mapping [pmm]  INNER JOIN #ProductIdsBeforeFiltersApplied 
		ON #ProductIdsBeforeFiltersApplied.ProductId = [pmm].ProductId 
	
		IF @SpecificationAttributesCount > 0  
		BEGIN 
			DELETE FROM #FilterableManufacturers  FROM #FilterableManufacturers fm  LEFT JOIN #FilteredSpecificationAttributesToProduct fsatp 
			ON fsatp.ProductId = fm.ProductId  WHERE fsatp.ProductId IS NULL OR fsatp.AttributesCount != @SpecificationAttributesCount 
		END 
	
		IF @ProductAttributesCount > 0  
		BEGIN 
			DELETE FROM #FilterableManufacturers  FROM #FilterableManufacturers fm  
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = fm.ProductId  WHERE   (  pibfa.ChildProductId = 0 AND  (  NOT EXISTS (SELECT NULL FROM #FilteredProductAttributesToProduct 
			WHERE ProductId = pibfa.ProductId)  OR  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ProductId) != @ProductAttributesCount  )  )  OR  (  pibfa.ChildProductId != 0 
			AND  (  NOT EXISTS (SELECT NULL FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ChildProductId)  OR  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct 
			WHERE ProductId = pibfa.ChildProductId) != @ProductAttributesCount  )  ) 
		END 

		IF @VendorsCount > 0  
			BEGIN DELETE FROM #FilterableManufacturers  WHERE NOT EXISTS  (  SELECT NULL FROM Product [p]  
			INNER JOIN #FilteredVendorIds [fv] ON fv.VendorId = [p].VendorId  WHERE [p].Id = #FilterableManufacturers.ProductId  ) 
		END 

		INSERT INTO #FilterableManufacturersDistinct ([ManufacturerId])  
		SELECT DISTINCT ManufacturerId  FROM #FilterableManufacturers 
		SELECT @FilterableManufacturerIds = COALESCE(@FilterableManufacturerIds + ',' , '') + CAST(ManufacturerId as nvarchar(4000))  FROM #FilterableManufacturersDistinct 

		CREATE TABLE #FilterableVendors   (  [ProductId] int NOT NULL,  [VendorId] int NOT NULL  ) 
		CREATE TABLE #FilterableVendorsDistinct   (  [VendorId] int NOT NULL  ) 
	 
		INSERT INTO #FilterableVendors ([ProductId], [VendorId]) 
		SELECT DISTINCT [pv].Id, [pv].VendorId  
		FROM Product [pv]  INNER JOIN #ProductIdsBeforeFiltersApplied ON #ProductIdsBeforeFiltersApplied.ProductId = [pv].Id 
	
		IF @SpecificationAttributesCount > 0  
		BEGIN 
			DELETE FROM #FilterableVendors  FROM #FilterableVendors fv  
			LEFT JOIN #FilteredSpecificationAttributesToProduct fsatp ON fsatp.ProductId = fv.ProductId  
			WHERE fsatp.ProductId IS NULL OR fsatp.AttributesCount != @SpecificationAttributesCount 
		END 
	
		IF @ProductAttributesCount > 0  
		BEGIN 
			DELETE FROM #FilterableVendors  FROM #FilterableVendors fv  
			INNER JOIN #ProductIdsBeforeFiltersApplied pibfa ON pibfa.ProductId = fv.ProductId  WHERE   (  pibfa.ChildProductId = 0 
			AND  (  NOT EXISTS (SELECT NULL FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ProductId)  
			OR  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ProductId) != @ProductAttributesCount  )  ) 
			OR  (  pibfa.ChildProductId != 0 AND  (  NOT EXISTS (SELECT NULL FROM #FilteredProductAttributesToProduct WHERE ProductId = pibfa.ChildProductId)  
			OR  (SELECT AttributesCount FROM #FilteredProductAttributesToProduct 
			WHERE ProductId = pibfa.ChildProductId) != @ProductAttributesCount  )  ) 
		END 

		IF @ManufacturersCount > 0  
		BEGIN DELETE FROM #FilterableVendors  
		WHERE NOT EXISTS  (  SELECT NULL FROM Product_Manufacturer_Mapping [pmm]  INNER JOIN #FilteredManufacturers [fm] ON [fm].ManufacturerId = [pmm].ManufacturerId  WHERE [pmm].ProductId = #FilterableVendors.ProductId  ) 
		END 

		INSERT INTO #FilterableVendorsDistinct ([VendorId])  SELECT DISTINCT VendorId  FROM #FilterableVendors SELECT @FilterableVendorIds = COALESCE(@FilterableVendorIds + ',' , '') + CAST(VendorId as nvarchar(4000))  
		FROM #FilterableVendorsDistinct 
	
		DROP TABLE #ProductIdsBeforeFiltersApplied  
		DROP TABLE #FilteredSpecificationAttributeOptions  
		DROP TABLE #FilterableSpecs  
		DROP TABLE #FilteredSpecificationAttributes  
		DROP TABLE #FilteredSpecificationAttributesToProduct  
		DROP TABLE #FilterableSpecsDistinct  
		DROP TABLE #PotentialProductSpecificationAttributeIds  
		DROP TABLE #FilteredProductVariantAttributes  
		DROP TABLE #FilteredProductAttributes  
		DROP TABLE #FilteredProductAttributesToProduct  
		DROP TABLE #FilterableProductVariantIds  
		DROP TABLE #FilterableProductVariantIdsDistinct  
		DROP TABLE #PotentialProductVariantAttributeIds  
		DROP TABLE #FilteredManufacturers  
		DROP TABLE #FilterableManufacturers  
		DROP TABLE #FilterableVendors  
		DROP TABLE #FilterableVendorsDistinct  
		DROP TABLE #FilterableManufacturersDistinct 
	END 
   
	DELETE #PageIndex 
	FROM #PageIndex LEFT OUTER JOIN ( SELECT MIN(IndexId) as RowId, ProductId  FROM #PageIndex  GROUP BY ProductId  ) AS KeepRows 
	ON #PageIndex.IndexId = KeepRows.RowId  
	WHERE KeepRows.RowId IS NULL   

	SET @TotalRecords = @TotalRecords - @@rowcount 
	
	CREATE TABLE #PageIndexDistinct   (  [IndexId] int IDENTITY (1, 1) NOT NULL,  [ProductId] int NOT NULL  ) 
	INSERT INTO #PageIndexDistinct ([ProductId])  SELECT [ProductId]  
	FROM #PageIndex  ORDER BY [IndexId] 
	
	IF @IsOnSaleFilterEnabled = 1  
	BEGIN   
		IF EXISTS (SELECT NULL FROM Product p LEFT JOIN Product cp ON p.Id = cp.ParentGroupedProductId INNER JOIN #PageIndexDistinct [pid] ON [pid].ProductId = p.Id 
		WHERE (  (cp.Id IS NULL AND p.OldPrice > 0 AND p.Price != p.OldPrice)  OR  (cp.Id IS NOT NULL AND cp.OldPrice > 0 AND cp.OldPrice != cp.Price)  ) )  
		BEGIN  
			SET @HasProductsOnSale = 1  
		END  
		ELSE  
			SET @HasProductsOnSale = 0  
	END
	 
	IF @IsInStockFilterEnabled = 1  
	BEGIN
		IF @IsErpAccount = 1 AND @PercentageOfStockAllowed < 100
		BEGIN
			IF EXISTS (SELECT NULL FROM Product p LEFT JOIN Product cp ON p.Id = cp.ParentGroupedProductId   
			INNER JOIN #PageIndexDistinct [pid] ON [pid].ProductId = p.Id  WHERE (  (cp.ID IS NULL  AND   (  (p.ManageInventoryMethodId = 0) OR  (P.ManageInventoryMethodId = 1 AND  (  (p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = p.Id AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND p.UseMultipleWarehouses = 1)  )  )  )  )  OR  (p.Id IS NOT NULL 
			AND   (  (cp.ManageInventoryMethodId = 0) OR  (cp.ManageInventoryMethodId = 1 AND  (  (cp.StockQuantity > 0 AND cp.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] 
			WHERE [pwi].ProductId = cp.Id 
			AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = @ErpSalesOrg_Id)  
			AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity  AND [dbo].[ErpPercentageOfStock](@ErpAccountId, @PercentageOfStockAllowed, [pwi].ProductId, [pwi].StockQuantity) >= 1) AND cp.UseMultipleWarehouses = 1)))))))  
			BEGIN  
				SET @HasProductsInStock = 1  
			END  
			ELSE  
				SET @HasProductsInStock = 0 
		END
		ELSE IF @IsErpAccount = 1
		BEGIN
			IF EXISTS (SELECT NULL FROM Product p LEFT JOIN Product cp ON p.Id = cp.ParentGroupedProductId   
			INNER JOIN #PageIndexDistinct [pid] ON [pid].ProductId = p.Id  WHERE (  (cp.ID IS NULL  AND   (  (p.ManageInventoryMethodId = 0) OR  (P.ManageInventoryMethodId = 1 AND  (  (p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = p.Id AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND p.UseMultipleWarehouses = 1)  )  )  )  )  OR  (p.Id IS NOT NULL 
			AND   (  (cp.ManageInventoryMethodId = 0) OR  (cp.ManageInventoryMethodId = 1 AND  (  (cp.StockQuantity > 0 AND cp.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] 
			WHERE [pwi].ProductId = cp.Id 
			AND [pwi].WarehouseId IN ( SELECT NopWarehouseId FROM [dbo].[ErpSalesOrgWarehouse] where ErpSalesOrgId = @ErpSalesOrg_Id)  
			AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND cp.UseMultipleWarehouses = 1)))))))  
			BEGIN  
				SET @HasProductsInStock = 1  
			END  
			ELSE  
				SET @HasProductsInStock = 0 
		END
		ELSE
		BEGIN
			IF EXISTS (SELECT NULL FROM Product p   LEFT JOIN Product cp ON p.Id = cp.ParentGroupedProductId   
			INNER JOIN #PageIndexDistinct [pid] ON [pid].ProductId = p.Id  WHERE (  (cp.ID IS NULL  AND   (  (p.ManageInventoryMethodId = 0) OR  (P.ManageInventoryMethodId = 1 AND  (  (p.StockQuantity > 0 AND p.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] WHERE [pwi].ProductId = p.Id AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity AND [dbo].[ErpPercentageOfStock](@ErpAccountId, @PercentageOfStockAllowed, [pwi].ProductId, [pwi].StockQuantity) >= 1) AND p.UseMultipleWarehouses = 1)  )  )  )  )  OR  (p.Id IS NOT NULL 
			AND   (  (cp.ManageInventoryMethodId = 0) OR  (cp.ManageInventoryMethodId = 1 AND  (  (cp.StockQuantity > 0 AND cp.UseMultipleWarehouses = 0) OR   
			(EXISTS(SELECT 1 FROM ProductWarehouseInventory [pwi] 
			WHERE [pwi].ProductId = cp.Id AND [pwi].StockQuantity > 0 AND [pwi].StockQuantity > [pwi].ReservedQuantity) AND cp.UseMultipleWarehouses = 1)))))))
			BEGIN  
				SET @HasProductsInStock = 1  
			END  
			ELSE  
				SET @HasProductsInStock = 0 
		END  
	END 
	
	SELECT TOP (@RowsToReturn)  p.*  FROM  #PageIndexDistinct [pi]  INNER JOIN Product p with (NOLOCK) on p.Id = [pi].[ProductId]  
	WHERE  [pi].IndexId > @PageLowerBound AND   [pi].IndexId < @PageUpperBound  
	ORDER BY  [pi].IndexId   
 
	DROP TABLE #ErpProductIdsByFacetFilter  
	DROP TABLE #KeywordProducts  
	DROP TABLE #FilteredCategoryIds  
	DROP TABLE #FilteredVendorIds  
	DROP TABLE #FilteredCustomerRoleIds  
	DROP TABLE #DisplayOrderTmp  
	DROP TABLE #PageIndex  
	DROP TABLE #PageIndexDistinct  
END