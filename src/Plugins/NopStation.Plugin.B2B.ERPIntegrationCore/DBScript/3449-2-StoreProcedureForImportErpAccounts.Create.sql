CREATE OR ALTER PROCEDURE [dbo].[ErpAccountImportProcedure]
(
	@CurrentUserId          INT = 0
)
AS
BEGIN
-- current UTCDateTime
DECLARE @CurrentUTCDATE datetime = GETUTCDATE();
-- Declare the variables to store the values returned by FETCH.  
DECLARE @AccountNumber nvarchar(50),
	@AccountName nvarchar(100),
	@SalesOrganisationCode nvarchar(100),
	@BillingFirstName nvarchar (max),
	@BillingLastName nvarchar (max),
	@BillingEmail nvarchar (max),
	@BillingCompany nvarchar (max),
	@BillingCountry nvarchar (max),
	@BillingStateProvince nvarchar (max),
	@BillingCity nvarchar (max),
	@BillingAddress1 nvarchar (max),
	@BillingAddress2 nvarchar (max),
	@BillingSuburb nvarchar (200),
	@BillingZipPostalCode nvarchar (max),
	@BillingPhoneNumber nvarchar (max),
	@VatNumber nvarchar (50),
	@CreditLimit nvarchar (100),
	@CurrentBalance nvarchar (100),
	@AllowOverspend nvarchar (10),
	@PriceGroupCode nvarchar (50),
	@PreFilterFacets nvarchar (500),
	@PaymentTypeCode nvarchar (10),
	@OverrideBackOrderingConfigSetting nvarchar (10),
	@AllowAccountsBackOrdering nvarchar (10),
	@OverrideAddressEditOnCheckoutConfigSetting nvarchar (10),
	@AllowAccountsAddressEditOnCheckout nvarchar (10),
	@StockDisplayFormatTypeId nvarchar (10),
	@ErpAccountStatusTypeId nvarchar (10),
	@PercentageOfStockAllowed nvarchar (10),
	@LastAccountRefresh nvarchar (max),
	@LastPriceRefresh nvarchar (max),
	@IsActive nvarchar (10),
	@IsDefaultPaymentAccount nvarchar (10);

	-- cursor for import account
	DECLARE account_import_cursor CURSOR FAST_FORWARD FOR  
	SELECT [AccountNumber]
			,[AccountName]
			,[SalesOrganisationCode]
			,[BillingFirstName]
			,[BillingLastName]
			,[BillingEmail]
			,[BillingCompany]
			,[BillingCountry]
			,[BillingStateProvince]
			,[BillingCity]
			,[BillingAddress1]
			,[BillingAddress2]
			,[BillingSuburb]
			,[BillingZipPostalCode]
			,[BillingPhoneNumber]
			,[VatNumber]
			,[CreditLimit]
			,[CurrentBalance]
			,[AllowOverspend]
			,[PriceGroupCode]
			,[PreFilterFacets]
			,[PaymentTypeCode]
			,[OverrideBackOrderingConfigSetting]
			,[AllowAccountsBackOrdering]
			,[OverrideAddressEditOnCheckoutConfigSetting]
			,[AllowAccountsAddressEditOnCheckout]
			,[StockDisplayFormatTypeId]
			,[ErpAccountStatusTypeId]
			,[PercentageOfStockAllowed]
			,[LastAccountRefresh]
			,[LastPriceRefresh]
			,[IsActive] FROM [dbo].[ErpAccountImport];

	OPEN account_import_cursor;

	-- Perform the first fetch
	FETCH NEXT FROM account_import_cursor INTO @AccountNumber, @AccountName, @SalesOrganisationCode, 
			@BillingFirstName, @BillingLastName, @BillingEmail, @BillingCompany, @BillingCountry, @BillingStateProvince, @BillingCity, 
			@BillingAddress1, @BillingAddress2, @BillingSuburb, @BillingZipPostalCode, @BillingPhoneNumber,
			@VatNumber, @CreditLimit, @CurrentBalance, @AllowOverspend, 
			@PriceGroupCode, @PreFilterFacets, @PaymentTypeCode, @OverrideBackOrderingConfigSetting, @AllowAccountsBackOrdering, 
			@OverrideAddressEditOnCheckoutConfigSetting, @AllowAccountsAddressEditOnCheckout, @StockDisplayFormatTypeId,
			@ErpAccountStatusTypeId, @PercentageOfStockAllowed, @LastAccountRefresh, @LastPriceRefresh, @IsActive;

	-- Check @@FETCH_STATUS to see if there are any more rows to fetch.  
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @currentSalesOrgId int = 0
		SET @currentSalesOrgId = (SELECT Top(1) ISNULL(Id, 0) FROM [dbo].[Erp_Sales_Org] salesOrg with (NOLOCK) where salesOrg.Code = @SalesOrganisationCode);

		DECLARE @currentPriceGroupCodeId int = 0
		SET @currentPriceGroupCodeId = (SELECT Top(1) Id FROM [dbo].[Erp_Group_Price_Code] priceGroup with (NOLOCK) where priceGroup.[Code] = @PriceGroupCode);

		-- if sales organisation is not exist we won't import that one
		IF @currentSalesOrgId IS NOT NULL AND @currentSalesOrgId > 0
		BEGIN
			DECLARE @currentAccountId int = 0
			SET @currentAccountId = (SELECT Top(1) Id FROM [dbo].[Erp_Account] account with (NOLOCK) where account.AccountNumber = @AccountNumber 
									AND account.ErpSalesOrgId = @currentSalesOrgId);

			-- if Account is not exist we will add those otherwise we will update
			IF @currentAccountId IS NULL OR @currentAccountId < 1
			-- Account is not exist begin
			BEGIN

			DECLARE @CountryId int = 0
			SET @CountryId = (SELECT TOP(1) Id FROM [dbo].[Country] c with (NOLOCK) where c.[Name] = @BillingCountry);

			DECLARE @StateProvinceId int = 0
			SET @StateProvinceId = (SELECT TOP(1) Id FROM [dbo].[StateProvince] s with (NOLOCK) where s.[Name] = @BillingStateProvince AND s.[CountryId] =  @CountryId);

			DECLARE @currentAddressId int = 0

			IF @CountryId > 0 AND @StateProvinceId > 0
				BEGIN
					INSERT INTO [dbo].[Address]
							([FirstName]
							,[LastName]
							,[Email]
							,[Company]
							,[CountryId]
							,[StateProvinceId]
							,[City]
							,[Address1]
							,[Address2]
							,[ZipPostalCode]
							,[PhoneNumber]
							,[CreatedOnUtc])
						VALUES
							(@BillingFirstName
							,@BillingLastName
							,@BillingEmail
							,@BillingCompany
							,@CountryId
							,@StateProvinceId
							,@BillingCity
							,@BillingAddress1
							,@BillingAddress2
							,@BillingZipPostalCode
							,@BillingPhoneNumber
							,@CurrentUTCDATE)
						SET @currentAddressId = SCOPE_IDENTITY()
				END

				INSERT INTO [dbo].[Erp_Account]
				([IsActive]
				,[CreatedOnUtc]
				,[CreatedById]
				,[UpdatedOnUtc]
				,[UpdatedById]
				,[IsDeleted]
				,[AccountNumber]
				,[AccountName]
				,[ErpSalesOrgId]
				,[BillingAddressId]
				,[BillingSuburb]
				,[VatNumber]
				,[CreditLimit]
				,[CurrentBalance]
				,[AllowOverspend]
				,[B2BPriceGroupCodeId]
				,[PreFilterFacets]
				,[PaymentTypeCode]
				,[OverrideBackOrderingConfigSetting]
				,[AllowAccountsBackOrdering]
				,[OverrideAddressEditOnCheckoutConfigSetting]
				,[AllowAccountsAddressEditOnCheckout]
				,[StockDisplayFormatTypeId]
				,[ErpAccountStatusTypeId]
				,[PercentageOfStockAllowed]
				,[LastErpAccountSyncDate]
				,[LastPriceRefresh]
				,[IsDefaultPaymentAccount])
			VALUES
				(CASE
					When @IsActive = 'True' OR @IsActive = 'true' OR @IsActive = 'TRUE' THEN 1
					ELSE 0
				END
				,@CurrentUTCDATE
				,@CurrentUserId
				,@CurrentUTCDATE
				,@CurrentUserId
				,0
				,@AccountNumber
				,@AccountName
				,@currentSalesOrgId
				,@currentAddressId
				,@BillingSuburb
				,@VatNumber
				,CAST(@CreditLimit AS DECIMAL(18, 4))
				,CAST(@CurrentBalance AS DECIMAL(18, 4))
				,CASE
					When @AllowOverspend = 'True' OR @AllowOverspend = 'true' OR @AllowOverspend = 'TRUE' THEN 1
					ELSE 0
				END
				,@currentPriceGroupCodeId
				,@PreFilterFacets
				,@PaymentTypeCode
				,CASE
					When @OverrideBackOrderingConfigSetting = 'True' OR @OverrideBackOrderingConfigSetting = 'true' OR @OverrideBackOrderingConfigSetting = 'TRUE' THEN 1
					ELSE 0
				END
				,CASE
					When @AllowAccountsBackOrdering = 'True' OR @AllowAccountsBackOrdering = 'true' OR @AllowAccountsBackOrdering = 'TRUE' THEN 1
					ELSE 0
				END
				,CASE
					When @OverrideAddressEditOnCheckoutConfigSetting = 'True' OR @OverrideAddressEditOnCheckoutConfigSetting = 'true' OR @OverrideAddressEditOnCheckoutConfigSetting = 'TRUE' THEN 1
					ELSE 0
				END
				,CASE
					When @AllowAccountsAddressEditOnCheckout = 'True' OR @AllowAccountsAddressEditOnCheckout = 'true' OR @AllowAccountsAddressEditOnCheckout = 'TRUE' THEN 1
					ELSE 0
				END
				,CAST(@StockDisplayFormatTypeId AS int)
				,CAST(@ErpAccountStatusTypeId AS int)
				,CAST(@PercentageOfStockAllowed AS decimal(18,4))
				,CASE WHEN @LastAccountRefresh IS NULL OR @LastAccountRefresh = '' THEN NULL ELSE CONVERT(datetime, @LastAccountRefresh) END
				,CASE WHEN @LastPriceRefresh IS NULL OR @LastPriceRefresh = '' THEN NULL ELSE CONVERT(datetime, @LastPriceRefresh) END
				,CASE When @IsDefaultPaymentAccount = 'True' OR @IsDefaultPaymentAccount = 'true' OR @IsDefaultPaymentAccount = 'TRUE' THEN 1
					ELSE 0
				END)
			END -- Account is not exist end
			ELSE
			-- Account is exist begin
			BEGIN
				SET @currentAddressId = (SELECT Top(1) BillingAddressId FROM [dbo].[Erp_Account] account with (NOLOCK) where account.Id = @currentAccountId);

				-- update address
				IF @currentAddressId IS NOT NULL AND @currentAddressId > 0
				BEGIN
					UPDATE [dbo].[Address]
					SET [FirstName] = @BillingFirstName
						,[LastName] = @BillingLastName
						,[Email] = @BillingEmail
						,[Company] = @BillingCompany
						,[CountryId] = @CountryId
						,[StateProvinceId] = @StateProvinceId
						,[City] = @BillingCity
						,[Address1] = @BillingAddress1
						,[Address2] = @BillingAddress2
						,[ZipPostalCode] = @BillingZipPostalCode
						,[PhoneNumber] = @BillingPhoneNumber
					WHERE Id = @currentAddressId
				END
				ELSE
				BEGIN
					INSERT INTO [dbo].[Address]
							([FirstName]
							,[LastName]
							,[Email]
							,[Company]
							,[CountryId]
							,[StateProvinceId]
							,[City]
							,[Address1]
							,[Address2]
							,[ZipPostalCode]
							,[PhoneNumber]
							,[CreatedOnUtc])
						VALUES
							(@BillingFirstName
							,@BillingLastName
							,@BillingEmail
							,@BillingCompany
							,@CountryId
							,@StateProvinceId
							,@BillingCity
							,@BillingAddress1
							,@BillingAddress2
							,@BillingZipPostalCode
							,@BillingPhoneNumber
							,@CurrentUTCDATE)
						SET @currentAddressId = SCOPE_IDENTITY()
				END
				UPDATE [dbo].[Erp_Account]
					SET [IsActive] = 
						CASE
							When @IsActive = 'True' OR @IsActive = 'true' OR @IsActive = 'TRUE' THEN 1
							ELSE 0
						END
						,[UpdatedOnUtc] = @CurrentUTCDATE
						,[UpdatedById] = @CurrentUserId
						,[AccountNumber] = @AccountNumber
						,[AccountName] = @AccountName
						,[ErpSalesOrgId] = @currentSalesOrgId
						,[BillingAddressId] = @currentAddressId
						,[BillingSuburb] = @BillingSuburb
						,[VatNumber] = @VatNumber
						,[CreditLimit] = CAST(@CreditLimit AS DECIMAL(18, 4))
						,[CurrentBalance] = CAST(@CurrentBalance AS DECIMAL(18, 4))
						,[AllowOverspend] = 
						CASE
							When @AllowOverspend = 'True' OR @AllowOverspend = 'true' OR @AllowOverspend = 'TRUE' THEN 1
							ELSE 0
						END
						,[B2BPriceGroupCodeId] = @currentPriceGroupCodeId
						,[PreFilterFacets] = @PreFilterFacets
						,[PaymentTypeCode] = @PaymentTypeCode
						,[OverrideBackOrderingConfigSetting] = 
						CASE
							When @OverrideBackOrderingConfigSetting = 'True' OR @OverrideBackOrderingConfigSetting = 'true' OR @OverrideBackOrderingConfigSetting = 'TRUE' THEN 1
							ELSE 0
						END
						,[AllowAccountsBackOrdering] = 
						CASE
							When @AllowAccountsBackOrdering = 'True' OR @AllowAccountsBackOrdering = 'true' OR @AllowAccountsBackOrdering = 'TRUE' THEN 1
							ELSE 0
						END
						,[OverrideAddressEditOnCheckoutConfigSetting] = 
						CASE
							When @OverrideAddressEditOnCheckoutConfigSetting = 'True' OR @OverrideAddressEditOnCheckoutConfigSetting = 'true' OR @OverrideAddressEditOnCheckoutConfigSetting = 'TRUE' THEN 1
							ELSE 0
						END
						,[AllowAccountsAddressEditOnCheckout] = 
						CASE
							When @AllowAccountsAddressEditOnCheckout = 'True' OR @AllowAccountsAddressEditOnCheckout = 'true' OR @AllowAccountsAddressEditOnCheckout = 'TRUE' THEN 1
							ELSE 0
						END
						,[StockDisplayFormatTypeId] = CAST(@StockDisplayFormatTypeId AS INT)
						,[ErpAccountStatusTypeId] = CAST(@ErpAccountStatusTypeId AS INT)
						,[LastErpAccountSyncDate] = CASE WHEN @LastAccountRefresh IS NULL OR @LastAccountRefresh = '' THEN NULL ELSE CONVERT(datetime, @LastAccountRefresh) END
						,[LastPriceRefresh] = CASE WHEN @LastPriceRefresh IS NULL OR @LastPriceRefresh = '' THEN NULL ELSE CONVERT(datetime, @LastPriceRefresh) END
						,[IsDefaultPaymentAccount] = CASE
							When @IsDefaultPaymentAccount = 'True' OR @IsDefaultPaymentAccount = 'true' OR @IsDefaultPaymentAccount = 'TRUE' THEN 1
							ELSE 0
						END
					WHERE Id = @currentAccountId
			END -- Account is exist end
		END -- sales org not exist end

		-- Perform the next fetch
		FETCH NEXT FROM account_import_cursor INTO @AccountNumber, @AccountName, @SalesOrganisationCode, 
			@BillingFirstName, @BillingLastName, @BillingEmail, @BillingCompany, @BillingCountry, @BillingStateProvince, @BillingCity, 
			@BillingAddress1, @BillingAddress2, @BillingSuburb, @BillingZipPostalCode, @BillingPhoneNumber,
			@VatNumber, @CreditLimit, @CurrentBalance, @AllowOverspend, 
			@PriceGroupCode, @PreFilterFacets, @PaymentTypeCode, @OverrideBackOrderingConfigSetting, @AllowAccountsBackOrdering, 
			@OverrideAddressEditOnCheckoutConfigSetting, @AllowAccountsAddressEditOnCheckout, @StockDisplayFormatTypeId,
			@ErpAccountStatusTypeId, @PercentageOfStockAllowed, @LastAccountRefresh, @LastPriceRefresh, @IsActive;
	END
		
	CLOSE account_import_cursor;  
	DEALLOCATE account_import_cursor; 

	TRUNCATE TABLE [dbo].[ErpAccountImport];
END
