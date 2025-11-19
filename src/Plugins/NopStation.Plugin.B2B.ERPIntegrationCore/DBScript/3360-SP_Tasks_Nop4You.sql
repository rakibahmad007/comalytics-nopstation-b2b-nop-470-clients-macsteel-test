-- =============================================
-- Procedure created from Nop4You Plugin
-- Name : Unpublish Products Without Prefilter
-- Group: Tasks
-- =============================================

CREATE OR ALTER procedure [dbo].[sp_unpublish_products_without_prefilter] as
begin
	declare @prefilterSpecAttId int = (select Value from setting where name = 'b2bb2cfeaturessettings.prefilterfacetspecificationattributeid');
	IF @prefilterSpecAttId IS NULL RAISERROR ('Could not determine id of Prefilter specification attribute', 10, 1) WITH NOWAIT;
	-- Unpublished product without any prefilter facet
	update p set p.Published = 0
		--select p.id, p.sku, p.name, p.published, p.admincomment
		from Product p
		where p.published = 1 and not exists (select 1 from
			Product_specificationAttribute_Mapping psam with (nolock)
			join SpecificationAttributeOption sao with (nolock) on sao.id = psam.SpecificationAttributeOptionId and sao.SpecificationAttributeId = @prefilterSpecAttId
			where psam.productid = p.id
		);
end;
GO




-- =============================================
-- Procedure created from Nop4You Plugin
-- Name : Close store if accidentally kept open
-- Group: Tasks
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[CloseStoreIfAccidentallyKeptOpen]
AS
BEGIN
    UPDATE [dbo].[Setting]
    SET [Value] = 'True'
    WHERE [Name] = 'storeinformationsettings.storeclosed';
END
GO




-- =============================================
-- Procedure created from Nop4You Plugin
-- Name : Unpublish categories without any products
-- Group: Tasks
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[UnpublishCategoriesWithoutAnyProducts]
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH categhierch AS (
        SELECT c.Id AS OwnId, c.ParentCategoryId AS AncestorId
        FROM Category c
        WHERE c.Deleted = 0

        UNION ALL

        SELECT child.OwnId, parent.ParentCategoryId AS AncestorId
        FROM categhierch child
        JOIN Category parent ON parent.Id = child.AncestorId AND child.AncestorId <> child.OwnId
        WHERE parent.Deleted = 0
    ),
    desired AS (
        SELECT c.Id,
            CASE
                WHEN c.Deleted = 1 THEN 0
                WHEN EXISTS (
                    SELECT 1
                    FROM Product_Category_Mapping pcm
                    JOIN Product p ON p.Id = pcm.ProductId
                    WHERE pcm.CategoryId = c.Id AND p.Published = 1 AND p.Deleted = 0
                ) THEN 1
                WHEN EXISTS (
                    SELECT 1
                    FROM categhierch h
                    JOIN Product_Category_Mapping pcm ON pcm.CategoryId = h.OwnId
                    JOIN Product p ON p.Id = pcm.ProductId
                    WHERE h.AncestorId = c.Id AND p.Published = 1 AND p.Deleted = 0
                ) THEN 1
                ELSE 0
            END AS shouldPublish
        FROM Category c
    )

    UPDATE c
    SET Published = d.shouldPublish
    FROM Category c
    JOIN desired d ON d.Id = c.Id
    WHERE c.Published <> d.shouldPublish;

    SELECT CONCAT(@@ROWCOUNT, ' rows affected') AS Done;
END
GO




-- =============================================
-- Procedure created from Nop4You Plugin
-- Name: Re-sort Specification Attribute Option for thickness and size
-- Group: Tasks
-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[ResortSpecificationAttributeOptions_Thickness_Size_Length_NominalBore]
AS
BEGIN
    DECLARE @Thickness INT = 0,
            @Size INT = 0,
            @Length INT = 0,
            @NominalBore INT = 0;
    
    SET @Size = (SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Size');
    SET @Thickness = (SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Thickness');
    SET @Length = (SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Length');
    SET @NominalBore = (SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Nominal Bore');
    
    IF (@Size > 0 AND @Thickness > 0 AND @Length > 0 AND @NominalBore > 0)
    BEGIN
        WITH Thickness_CTE (SortedDisplayOrder, SpecificationAttributeOptionId) AS
        (
            -- Thickness sorting
            SELECT
                ROW_NUMBER() OVER (ORDER BY sorted_name) AS SortedDisplayOrder,
                tempThickness.Id
            FROM (
                SELECT
                    Id,
                    [dbo].[StingToDecimalForUOM](Name) AS sorted_name
                FROM SpecificationAttributeOption
                WHERE SpecificationAttributeId = @Thickness
            ) AS tempThickness
            
            UNION
            
            -- Length sorting
            SELECT
                ROW_NUMBER() OVER (ORDER BY sorted_name) AS SortedDisplayOrder,
                tempLength.Id
            FROM (
                SELECT
                    Id,
                    [dbo].[StingToDecimalForUOM](Name) AS sorted_name
                FROM SpecificationAttributeOption
                WHERE SpecificationAttributeId = @Length
            ) AS tempLength
            
            UNION
            
            -- Nominal Bore sorting
            SELECT
                ROW_NUMBER() OVER (ORDER BY sorted_name) AS SortedDisplayOrder,
                tempNominalBore.Id
            FROM (
                SELECT
                    Id,
                    [dbo].[StingToDecimalForUOM](Name) AS sorted_name
                FROM SpecificationAttributeOption
                WHERE SpecificationAttributeId = @NominalBore
            ) AS tempNominalBore
            
            UNION
            
            -- Size sorting (dimensional)
            SELECT
                ROW_NUMBER() OVER (ORDER BY dim_1, dim_2, dim_3) AS SortedDisplayOrder,
                tempSize.Id
            FROM (
                SELECT
                    Id,
                    TRY_CAST(ISNULL(REVERSE(PARSENAME(REPLACE(REPLACE(REVERSE(Name), ' x ', '.'), '00.', ''), 1)), '') AS INT) AS dim_1,
                    TRY_CAST(ISNULL(REVERSE(PARSENAME(REPLACE(REPLACE(REVERSE(Name), ' x ', '.'), '00.', ''), 2)), '') AS INT) AS dim_2,
                    TRY_CAST(ISNULL(REVERSE(PARSENAME(REPLACE(REPLACE(REVERSE(Name), ' x ', '.'), '00.', ''), 3)), '') AS INT) AS dim_3
                FROM SpecificationAttributeOption
                WHERE SpecificationAttributeId = @Size
            ) AS tempSize
        )

        UPDATE saot
        SET saot.DisplayOrder = tcte.SortedDisplayOrder
        FROM SpecificationAttributeOption saot
        JOIN Thickness_CTE tcte ON saot.Id = tcte.SpecificationAttributeOptionId;
    END;
    
    -- ********************** viewing display order ***********************************
    SELECT *
    FROM [dbo].[SpecificationAttributeOption]
    WHERE SpecificationAttributeId = (
        SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Size'
    )
    
    UNION
    
    SELECT *
    FROM [dbo].[SpecificationAttributeOption]
    WHERE SpecificationAttributeId = (
        SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Thickness'
    )
    
    UNION
    
    SELECT *
    FROM [dbo].[SpecificationAttributeOption]
    WHERE SpecificationAttributeId = (
        SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Length'
    )
    
    UNION
    
    SELECT *
    FROM [dbo].[SpecificationAttributeOption]
    WHERE SpecificationAttributeId = (
        SELECT TOP 1 Id FROM SpecificationAttribute WHERE Name = 'Nominal Bore'
    )
    ORDER BY DisplayOrder, SpecificationAttributeId;
END
GO

CREATE OR ALTER FUNCTION [dbo].[StingToDecimalForUOM] 
    ( @valuestg varchar(50) )
    RETURNS  DECIMAL(9,2) 
    AS
    BEGIN
	    -- Declare the return variable here
	    DECLARE @value DECIMAL(9,2) 
	    set @value=0
	    if @valuestg like '%mm%'
		    begin  
			    set @value = CAST(isnull(replace(@valuestg, 'mm', ''), '') as DECIMAL(9,2)) 
		    end
	    else if @valuestg like '%cm%'
		    begin 
			    set @value = CAST(isnull(replace(@valuestg, 'cm', ''), '') as DECIMAL(9,2))*10
		    end
	    else if @valuestg like '%m%'
		    begin 
			    set @value = CAST(isnull(replace(@valuestg, 'm', ''), '') as DECIMAL(9,2))*1000
		    end
	    else  
		        set @value = CAST(isnull(@valuestg, '') as DECIMAL(9,2))  
	    return @value 
    END