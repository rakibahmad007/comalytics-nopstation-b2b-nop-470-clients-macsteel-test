-- NOTE: Macsteel 4.2 LIVE doesn't use it anymore

CREATE OR ALTER PROCEDURE [dbo].[sp_SpecialCategroyRemoveforMacsteelQuote]
AS
BEGIN  
    DECLARE @OrgId INT;
	DECLARE @productToQuote float;
	SELECT @productToQuote = CAST([Value] AS FLOAT)
	FROM [Setting] WHERE [Name] = 'b2bb2cfeaturessettings.productquoteprice';
    DECLARE org_cursor CURSOR FOR
    SELECT Id  FROM Erp_Sales_Org
    WHERE SpecialsCategoryId IS NOT NULL;
 
    OPEN org_cursor;
    FETCH NEXT FROM org_cursor INTO @OrgId;
 
    WHILE @@FETCH_STATUS = 0
    BEGIN 
		DELETE FROM Product_Category_Mapping 
        WHERE Id IN(
            SELECT pc.Id
            FROM Product_Category_Mapping pc
            JOIN Erp_Sales_Org so ON so.SpecialsCategoryId = pc.CategoryId
            WHERE so.id=@OrgId and pc.ProductId IN(
                SELECT p.Id
                FROM Product p
                LEFT JOIN Erp_Special_Price esp ON esp.NopProductId = p.Id
                WHERE esp.Price = @productToQuote
                AND esp.ErpAccountId IN(
                    SELECT a.Id
                    FROM Erp_Account a
                    WHERE (a.ErpSalesOrgId = @OrgId AND a.IsActive = 1 AND a.IsDeleted = 0)
                )
            )
        );
 
        FETCH NEXT FROM org_cursor INTO @OrgId ;
    END;
 
    CLOSE org_cursor;
    DEALLOCATE org_cursor; 
END