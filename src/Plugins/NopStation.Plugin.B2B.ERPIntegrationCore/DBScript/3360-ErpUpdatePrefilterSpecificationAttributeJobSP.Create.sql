CREATE OR ALTER PROCEDURE [dbo].[ErpUpdatePrefilterSpecificationAttributeJobSP]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @prefilterSpecAttributeId int = 0;

	-- current UTCDateTime
	DECLARE @CurrentUTCDATE datetime2 = GETUTCDATE();
	declare @countAddNeeded int = 0;
	declare @countActualAdded int = 0;
	declare @countdeleted int = 0;

	select @prefilterSpecAttributeId = Id
	from SpecificationAttribute
	where [Name] like 'Prefilter'

	IF @prefilterSpecAttributeId > 0
	BEGIN
    
      -- for all sales org warehouses
		CREATE TABLE #SowhTemp
		(
			NopwarehouseId int,
			B2BSalesOrganisationId int,
			WarehouseCode nvarchar(100)			
		);

		-- for full info
		CREATE TABLE #InfoTempTable
		(
			ProductId int,
			Code nvarchar(100),
			InUse bit,
			AttributeOptionId int,
			AttributeOptionName nvarchar(max)
		);

		-- attribute option need to add
		CREATE TABLE #NeedAttributeInsert
		(
			ProductId int,
			Code nvarchar(100)
		);

		DECLARE @ProductId int;
		DECLARE ProductCursor CURSOR FAST_FORWARD FOR
		SELECT DISTINCT P.Id from dbo.Product P WITH (NOLOCK) WHERE P.[Deleted] = 0
		EXCEPT (SELECT ProductId FROM dbo.Erp_Special_Includes_And_Excludes WHERE SpecialTypeId = 10 AND IsActive = 1);    
	
		INSERT INTO #SowhTemp (NopwarehouseId, B2BSalesOrganisationId, WarehouseCode)
		SELECT 
			map.NopWarehouseId,
			map.ErpSalesOrgId,
			map.WarehouseCode
		FROM dbo.Erp_Warehouse_Sales_Org_Map AS map;

		OPEN ProductCursor;
		FETCH NEXT FROM ProductCursor
		INTO @ProductId;

		WHILE @@FETCH_STATUS = 0
		BEGIN -- product loop start

		-- Product Id, warehouse code, whether the product has stock entry in nop warehouse inventory. Note: a product can be in multiple warehouses.
			With WareHouseInUse_CTE(ProductId, Code, InUse)
			AS
			(
				SELECT DISTINCT ProductId = @ProductId, #SowhTemp.[WarehouseCode],
				CASE WHEN EXISTS (SELECT Id FROM ProductWarehouseInventory with (NOLOCK) where ProductWarehouseInventory.ProductId = @ProductId and ProductWarehouseInventory.WarehouseId = Warehouse.Id)
				   THEN 1
				   ELSE 0
				END AS InUse
				FROM Warehouse
				INNER JOIN #SowhTemp ON Warehouse.[Id] = #SowhTemp.[NopWarehouseId]
			),
			CurrentSpecificationAttribute_CTE(AttributeOptionId, AttributeOptionName) 
			AS
			(
				SELECT sao.Id
					  ,sao.[Name]
				  FROM [dbo].[Product_SpecificationAttribute_Mapping] psam
				  JOIN SpecificationAttributeOption sao ON psam.[SpecificationAttributeOptionId] = sao.Id
				  where [ProductId] = @ProductId AND sao.[SpecificationAttributeId] = @prefilterSpecAttributeId
			)
			INSERT INTO #InfoTempTable (ProductId, Code, InUse, AttributeOptionId, AttributeOptionName)
			SELECT DISTINCT wh.ProductId, wh.Code, wh.InUse, csa.AttributeOptionId, csa.AttributeOptionName from WareHouseInUse_CTE wh
			LEFT JOIN CurrentSpecificationAttribute_CTE  csa ON wh.CODE = csa.AttributeOptionName;

			INSERT INTO #NeedAttributeInsert (ProductId, Code)
			SELECT ProductId, Code FROM #InfoTempTable  WHERE AttributeOptionId IS NULL AND InUse = 1;

			--Select * from #InfoTempTable;

			DELETE FROM Product_SpecificationAttribute_Mapping WHERE [ProductId] = @ProductId AND SpecificationAttributeOptionId IN (Select AttributeOptionId FROM #InfoTempTable WHERE AttributeOptionId IS NOT NULL AND InUse = 0);

			set @countdeleted = @@ROWCOUNT

			TRUNCATE TABLE #InfoTempTable;

			FETCH NEXT FROM ProductCursor
			INTO @ProductId;

		END	-- product loop end

		DROP TABLE #InfoTempTable;
		CLOSE ProductCursor;
		DEALLOCATE ProductCursor;

		---- add specification attribute and specification attribute mapping

		DECLARE @specAttrOptionId int = 0;

		DECLARE @AttrProductId int, @Code varchar(100);
		DECLARE AtrributeCursor CURSOR FAST_FORWARD FOR
		SELECT ProductId, Code from #NeedAttributeInsert;

		OPEN AtrributeCursor;
		FETCH NEXT FROM AtrributeCursor
		INTO @AttrProductId, @Code;

		WHILE @@FETCH_STATUS = 0
		BEGIN -- attribute loop start
			SELECT @specAttrOptionId = Id FROM SpecificationAttributeOption with (NOLOCK) WHERE SpecificationAttributeId = @prefilterSpecAttributeId AND [Name] = @Code;
			-- if not exist we should add
			IF @specAttrOptionId = 0 OR @specAttrOptionId IS NULL
			BEGIN
				INSERT INTO [dbo].[SpecificationAttributeOption]
			   ([SpecificationAttributeId]
			   ,[Name]
			   ,[ColorSquaresRgb]
			   ,[DisplayOrder])
		 VALUES
			   (@prefilterSpecAttributeId
			   ,@Code
			   ,null
			   ,0);
			   -- after insert select now
			   SELECT @specAttrOptionId = Id FROM SpecificationAttributeOption with (NOLOCK) WHERE SpecificationAttributeId = @prefilterSpecAttributeId AND [Name] = @Code;
			 END
			---- add specification attribute mapping
			DECLARE @SpecificationAttributeMappingId int = 0;
			SELECT @SpecificationAttributeMappingId = Id FROM Product_SpecificationAttribute_Mapping with (NOLOCK) WHERE ProductId = @AttrProductId AND SpecificationAttributeOptionId = @specAttrOptionId;
			-- check if same mapping exist or not (we will only add if not exist)
			IF @SpecificationAttributeMappingId = 0 OR @SpecificationAttributeMappingId IS NULL
			BEGIN
				INSERT INTO Product_SpecificationAttribute_Mapping (ProductId, AttributeTypeId, SpecificationAttributeOptionId, AllowFiltering, ShowOnProductPage, DisplayOrder)
				VALUES (@AttrProductId,0, @specAttrOptionId, 0, 0, 0);

				SET @countActualAdded = @countActualAdded + 1;
			END
			FETCH NEXT FROM AtrributeCursor
			INTO @AttrProductId, @Code;
		END -- attribute loop end

		--SELECT ProductId, Code from #NeedAttributeInsert;
		SET @countAddNeeded = (SELECT COUNT(*) AS int from #NeedAttributeInsert);

		INSERT INTO [dbo].[Log]
           ([LogLevelId]
           ,[ShortMessage]
           ,[FullMessage]
           ,[IpAddress]
           ,[CustomerId]
           ,[PageUrl]
           ,[ReferrerUrl]
           ,[CreatedOnUtc])
     VALUES
           (20
           ,'Prefilter Specification Attribute Updated Succesfully,Add needed for ' + CAST(@countAddNeeded AS VARCHAR) + ', Added: ' + CAST(@countActualAdded AS VARCHAR) + ', Deleted: '+ CAST(@countdeleted AS VARCHAR)
           ,null
           ,null
           ,null
           ,null
           ,null
           ,@CurrentUTCDATE);

		DROP TABLE #NeedAttributeInsert;
		CLOSE AtrributeCursor;
		DEALLOCATE AtrributeCursor;
	END
END