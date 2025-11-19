CREATE OR ALTER PROCEDURE [dbo].[ErpNopUserImportProcedure]
(
    @CurrentUserId INT = 0
)
AS
BEGIN
    -- current UTCDateTime
    DECLARE @CurrentUTCDATE datetime = GETUTCDATE();

    -- Declare the variables to store the values returned by FETCH.  
    DECLARE @Email nvarchar (1000),
            @AccountNumber nvarchar (50),
            @AccountName nvarchar (100),
            @AccountSalesOrganisationCode nvarchar(100),
            @ShipToCode nvarchar (50),
            @ShipToName nvarchar (100),
            @IsActive nvarchar (10),
            @ErpUserType nvarchar (100);  -- ✅ new variable

    -- cursor for import user
    DECLARE user_import_cursor CURSOR FAST_FORWARD FOR  
    SELECT [Email],
           [AccountNumber],
           [AccountName],
           [AccountSalesOrganisationCode],
           [ShipToCode],
           [ShipToName],
           [IsActive],
           [ErpUserType]   -- ✅ include new column
    FROM [dbo].[ErpNopUserImport]; -- ✅ renamed table

    OPEN user_import_cursor;

    -- Perform the first fetch
    FETCH NEXT FROM user_import_cursor INTO 
        @Email, @AccountNumber, @AccountName, @AccountSalesOrganisationCode, 
        @ShipToCode, @ShipToName, @IsActive, @ErpUserType;

    -- Check @@FETCH_STATUS to see if there are any more rows to fetch.  
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @customerId int = 0;
        SET @customerId = (SELECT TOP(1) Id 
                           FROM [dbo].[Customer] customer WITH (NOLOCK) 
                           WHERE customer.Email = @Email);

        DECLARE @currentAccountSalesOrgId int = 0;
        SET @currentAccountSalesOrgId = (SELECT TOP(1) ISNULL(Id, 0) 
                                         FROM [dbo].[Erp_Sales_Org] salesOrg WITH (NOLOCK) 
                                         WHERE salesOrg.Code = @AccountSalesOrganisationCode);

        DECLARE @accountId int = 0;
        SET @accountId = (SELECT TOP(1) Id 
                          FROM [dbo].[Erp_Account] account WITH (NOLOCK) 
                          WHERE account.AccountNumber = @AccountNumber 
                            AND account.ErpSalesOrgId = @currentAccountSalesOrgId);

        DECLARE @shipToId int = 0;
        SET @shipToId = (SELECT TOP(1) Id 
                         FROM [dbo].[Erp_ShipToAddress] shipto WITH (NOLOCK) 
                         WHERE shipto.ShipToCode = @ShipToCode);

        IF @customerId > 0 AND @accountId > 0 AND @shipToId > 0
        BEGIN
            -- as one customer won't have multiple b2b user so, we check b2b user by customer id
            DECLARE @currentUsertId int = 0;
            SET @currentUsertId = (SELECT TOP(1) Id 
                                   FROM [dbo].[Erp_Nop_User] userInfo WITH (NOLOCK) 
                                   WHERE userInfo.NopCustomerId = @customerId);

            -- if User is not exist we will add those otherwise we will update
            IF @currentUsertId IS NULL OR @currentUsertId < 1
            BEGIN
                INSERT INTO [dbo].[Erp_Nop_User]
                    ([IsActive],
                     [CreatedOnUtc],
                     [CreatedById],
                     [UpdatedOnUtc],
                     [UpdatedById],
                     [IsDeleted],
                     [NopCustomerId],
                     [ErpAccountId],
                     [ErpShipToAddressId],
                     [BillingErpShipToAddressId],
                     [ShippingErpShipToAddressId],
                     [ErpUserTypeId])  -- ✅ insert new column
                VALUES
                    (CASE WHEN @IsActive = 'True' OR @IsActive = 'true' OR @IsActive = 'TRUE' THEN 1 ELSE 0 END,
                     @CurrentUTCDATE,
                     @CurrentUserId,
                     @CurrentUTCDATE,
                     @CurrentUserId,
                     0,
                     @customerId,
                     @accountId,
                     @shipToId,
                     0,
                     0,
                     CASE WHEN @ErpUserType = 'B2BUser' THEN 5 ELSE 10 END)  -- ✅ value from import table
            END
            ELSE
            BEGIN
                UPDATE [dbo].[Erp_Nop_User]
                SET [IsActive] = CASE WHEN @IsActive = 'True' OR @IsActive = 'true' OR @IsActive = 'TRUE' THEN 1 ELSE 0 END,
                    [UpdatedOnUtc] = @CurrentUTCDATE,
                    [UpdatedById] = @CurrentUserId,
                    [ErpAccountId] = @accountId,
                    [ErpShipToAddressId] = @shipToId,
                    [ErpUserTypeId] = CASE WHEN @ErpUserType = 'B2BUser' THEN 5 ELSE 10 END  -- ✅ update new column
                WHERE Id = @currentUsertId;
            END
        END
     
        FETCH NEXT FROM user_import_cursor INTO 
            @Email, @AccountNumber, @AccountName, @AccountSalesOrganisationCode, 
            @ShipToCode, @ShipToName, @IsActive, @ErpUserType;
    END

    CLOSE user_import_cursor;  
    DEALLOCATE user_import_cursor; 

    TRUNCATE TABLE [dbo].[ErpNopUserImport]; -- ✅ renamed table
END
