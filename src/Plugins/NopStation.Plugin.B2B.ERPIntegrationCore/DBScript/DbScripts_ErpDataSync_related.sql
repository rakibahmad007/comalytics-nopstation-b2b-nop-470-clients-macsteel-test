-- needs after product sync

CREATE OR ALTER PROCEDURE [dbo].[sp_set_spec_attr_display_order_alphabetically] AS
BEGIN
    -- Set display order of specification attribute options alphabetically
    with wanted as (
            select Id, SpecificationAttributeId, Name, row_number() over (partition by SpecificationAttributeId order by Name) as RN
            from specificationattributeoption)
        update target set DisplayOrder = wanted.RN
        from specificationattributeoption target join wanted on wanted.Id = target.Id;
END
GO

-- needs after product sync - SP to update skip price and stock for products based on category IDs
CREATE OR ALTER PROCEDURE [dbo].[UpdateSkipPriceStockProduct] 
(
	@dateTime datetime =null
)
AS
BEGIN
	DECLARE @Fect nvarchar(50) 
	create table #dataList ( [ids] nvarchar (max) 	)

	create table #idList ( [id] int not null )

	insert  into #dataList (ids) (
	select value  from [dbo].[Setting] 
	where name ='b2bb2cfeaturessettings.skiplivepricecheckcategoryids'
	or name = 'b2bb2cfeaturessettings.skiplivestockcheckcategoryids')

	-- Loop through each row in #dataList
	DECLARE data_cursor CURSOR FAST_FORWARD FOR  
	SELECT ids FROM #dataList;

	open data_cursor;

	FETCH NEXT FROM data_cursor INTO @Fect

	while @@FETCH_STATUS =0
	BEGIN
		INSERT INTO #idList (id)
		SELECT Split.a.value('.', 'INT')
		FROM
		(
			SELECT CAST('<X>'+REPLACE(@Fect, ',', '</X><X>')+'</X>' AS XML) AS String
		) AS A
		CROSS APPLY String.nodes('/X') AS Split(a);
		FETCH NEXT FROM data_cursor INTO @Fect;
		END
		CLOSE data_cursor;
		DEALLOCATE data_cursor; 

		update product set UpdatedOnUtc=@dateTime where id in (
		select p.Id from product p
		left join [dbo].[Product_Category_Mapping] pcm on p.id=pcm.ProductId
		where  pcm.categoryId in (
		select Id from #idList GROUP BY id
		union
		select c.id from category  c 
		where c.[ParentCategoryId] in (select Id from #idList)))
		
		drop table #idList
		drop table #dataList
	END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_erpstock_run_finished] AS
BEGIN
    EXEC sp_remove_stockless_products_from_specials;
    EXEC sp_remove_products_from_specials_of_wrong_warehouse;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_remove_stockless_products_from_specials]
AS BEGIN
	WITH
		specialCategories AS (
			SELECT DISTINCT SpecialsCategoryId FROM Erp_Sales_Org
		),
		specialProducts AS (
			SELECT pcm.Id 
			AS PcmId, pcm.ProductId 
			FROM specialCategories c 
			JOIN Product_Category_Mapping pcm 
			ON pcm.CategoryId = c.SpecialsCategoryId
		),
		stock AS (
			SELECT p.ProductId, SUM(pwi.StockQuantity) StockQuantity
			FROM specialProducts p JOIN ProductWarehouseInventory pwi ON pwi.ProductId = p.ProductId
			GROUP BY p.ProductId
		)
	--SELECT *
	DELETE pcm
	FROM stock s
	JOIN specialProducts sp ON s.ProductId = sp.ProductId
	JOIN Product_Category_Mapping pcm ON pcm.Id = sp.PcmId
	WHERE s.StockQuantity <= 0
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_remove_products_from_specials_of_wrong_warehouse] AS
BEGIN
    DELETE p
    FROM product p
    JOIN (
        SELECT pcm.ProductId
        FROM Erp_Sales_Org so
        JOIN Category specials ON specials.Id = so.SpecialsCategoryId
        JOIN Product_Category_Mapping pcm ON pcm.CategoryId = specials.Id
    ) subquery1 ON subquery1.ProductId = p.Id
    LEFT JOIN (
        SELECT pwi.ProductId
        FROM Erp_Warehouse_Sales_Org_Map wsom
        JOIN Warehouse w ON w.Id = wsom.NopWarehouseId
        JOIN ProductWarehouseInventory pwi ON pwi.WarehouseId = w.Id
    ) subquery2 ON subquery2.ProductId = p.Id
    WHERE subquery2.ProductId IS NULL
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateSpecAttrDisplayOrderByName] AS
BEGIN
    -- Define specification keywords
    DECLARE @specs TABLE (keyword VARCHAR(50), display_order INT);

    INSERT INTO @specs VALUES 
    ('grade', 1),
    ('schedule', 2),
    ('size', 3),
    ('thickness', 4),
    ('length', 5),
    ('nominal bore', 20),
    ('face finish', 30),
    ('color', 100),
    ('other', 110);

    -- Update specification attribute display orders
    UPDATE sa
    SET DisplayOrder = s.display_order
    FROM [dbo].[SpecificationAttribute] sa
    INNER JOIN @specs s ON LOWER(sa.Name) LIKE '%' + s.keyword + '%';
END;
GO

-- needs after product sync

CREATE OR ALTER PROCEDURE [dbo].[sp_erpproduct_run_finished] AS
BEGIN
 EXEC sp_set_spec_attr_display_order_alphabetically;

 -- to sort the specification attribute options
 EXEC ResortSpecificationAttributeOptions_Thickness_Size_Length_NominalBore;

 -- to udpate the display orders
 EXEC sp_UpdateSpecAttrDisplayOrderByName;
END
GO

