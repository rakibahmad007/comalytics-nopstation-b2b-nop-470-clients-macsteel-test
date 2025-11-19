CREATE OR ALTER PROCEDURE [dbo].[ErpShipToAddressImportProcedure]
(
	@CurrentUserId          INT = 0
)
AS
BEGIN
	DECLARE @CurrentUTCDATE datetime = GETUTCDATE();

	DECLARE @ShipToCode nvarchar (50),
		@ShipToName nvarchar (100),
		@Company nvarchar (max),
		@Country nvarchar (max),
		@StateProvince nvarchar (max),
		@City nvarchar (max),
		@Address1 nvarchar (max),
		@Address2 nvarchar (max),
		@Suburb nvarchar (200),
		@ZipPostalCode nvarchar (max),
		@PhoneNumber nvarchar (max),
		@DeliveryNotes nvarchar (max),
		@EmailAddresses nvarchar (max),
		@AccountNumber nvarchar (50),
		@AccountSalesOrganisationCode nvarchar(100),
		@IsActive nvarchar (10);

	DECLARE shipto_import_cursor CURSOR FAST_FORWARD FOR  
	SELECT [ShipToCode]
			,[ShipToName]
			,[Company]
			,[Country]
			,[StateProvince]
			,[City]
			,[Address1]
			,[Address2]
			,[Suburb]
			,[ZipPostalCode]
			,[PhoneNumber]
			,[DeliveryNotes]
			,[EmailAddresses]
			,[AccountNumber]
			,[AccountSalesOrganisationCode]
			,[IsActive]
	FROM [dbo].[ErpShipToAddressImport];

	OPEN shipto_import_cursor;

	FETCH NEXT FROM shipto_import_cursor INTO 
		@ShipToCode, @ShipToName, @Company, @Country, @StateProvince, @City, 
		@Address1, @Address2, @Suburb, @ZipPostalCode, @PhoneNumber, @DeliveryNotes, 
		@EmailAddresses, @AccountNumber, @AccountSalesOrganisationCode, @IsActive;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @CountryId int = (SELECT TOP(1) Id FROM [dbo].[Country] c with (NOLOCK) where c.[Name] = @Country);
		DECLARE @StateProvinceId int = (SELECT TOP(1) Id FROM [dbo].[StateProvince] s with (NOLOCK) where s.[Name] = @StateProvince AND s.[CountryId] =  @CountryId);

		DECLARE @currentAccountSalesOrgId int = (SELECT Top(1) Id FROM [dbo].[Erp_Sales_Org] salesOrg with (NOLOCK) where salesOrg.Code = @AccountSalesOrganisationCode);

		DECLARE @accountId int = (SELECT Top(1) Id FROM [dbo].[Erp_Account] account with (NOLOCK) 
								  where account.AccountNumber = @AccountNumber 
								    AND account.ErpSalesOrgId = @currentAccountSalesOrgId);

		DECLARE @currentAddressId int = 0;

		IF @CountryId > 0 AND @StateProvinceId > 0 AND @accountId > 0
		BEGIN
			DECLARE @currentShipToAddressId int = (SELECT Top(1) Id 
												   FROM [dbo].[Erp_ShipToAddress] shipto with (NOLOCK) 
												   where shipto.ShipToCode = @ShipToCode 
												     and ShipToAddressCreatedByTypeId = 10);

			IF @currentShipToAddressId IS NULL OR @currentShipToAddressId < 1
			BEGIN
				-- Insert Address
				INSERT INTO [dbo].[Address]
					([Company],[CountryId],[StateProvinceId],[City],
					 [Address1],[Address2],[ZipPostalCode],[PhoneNumber],[CreatedOnUtc])
				VALUES
					(@Company,@CountryId,@StateProvinceId,@City,
					 @Address1,@Address2,@ZipPostalCode,@PhoneNumber,@CurrentUTCDATE);

				SET @currentAddressId = SCOPE_IDENTITY();

				IF @currentAddressId > 0
				BEGIN
					-- Insert ShipTo
					INSERT INTO [dbo].[Erp_ShipToAddress]
						([IsActive],[CreatedOnUtc],[CreatedById],[UpdatedOnUtc],[UpdatedById],
						 [IsDeleted],[ShipToCode],[ShipToName],[AddressId],[Suburb],
						 [DeliveryNotes],[EmailAddresses],[ShipToAddressCreatedByTypeId],[OrderId])
					VALUES
						(CASE WHEN @IsActive IN ('True','true','TRUE') THEN 1 ELSE 0 END,
						 @CurrentUTCDATE,@CurrentUserId,@CurrentUTCDATE,@CurrentUserId,
						 0,@ShipToCode,@ShipToName,@currentAddressId,@Suburb,
						 @DeliveryNotes,@EmailAddresses,10,0);

					SET @currentShipToAddressId = SCOPE_IDENTITY();

					-- Insert Mapping
					INSERT INTO [dbo].[Erp_ShiptoAddress_Erp_Account_Map]
						([ErpAccountId],[ErpShiptoAddressId],[ErpShipToAddressCreatedByTypeId])
					VALUES
						(@accountId,@currentShipToAddressId,10);
				END
			END
			ELSE
			BEGIN
				-- Update existing address
				SET @currentAddressId = (SELECT Top(1) AddressId FROM [dbo].[Erp_ShipToAddress] WHERE Id = @currentShipToAddressId);

				UPDATE [dbo].[Address]
				SET [Company] = @Company,
					[CountryId] = @CountryId,
					[StateProvinceId] = @StateProvinceId,
					[City] = @City,
					[Address1] = @Address1,
					[Address2] = @Address2,
					[ZipPostalCode] = @ZipPostalCode,
					[PhoneNumber] = @PhoneNumber
				WHERE Id = @currentAddressId;

				-- Update ShipTo
				UPDATE [dbo].[Erp_ShipToAddress]
				SET [IsActive] = CASE WHEN @IsActive IN ('True','true','TRUE') THEN 1 ELSE 0 END,
					[UpdatedOnUtc] = @CurrentUTCDATE,
					[UpdatedById] = @CurrentUserId,
					[ShipToName] = @ShipToName,
					[AddressId] = @currentAddressId,
					[Suburb] = @Suburb,
					[DeliveryNotes] = @DeliveryNotes,
					[EmailAddresses] = @EmailAddresses
				WHERE Id = @currentShipToAddressId;


				IF EXISTS (
					SELECT 1 
					FROM Erp_ShiptoAddress_Erp_Account_Map
					WHERE ErpShiptoAddressId = @currentShipToAddressId
				)
				BEGIN
					-- Update the existing mapping
					UPDATE Erp_ShiptoAddress_Erp_Account_Map
					SET ErpAccountId = @accountId,
					    ErpShipToAddressCreatedByTypeId = 10
					WHERE ErpShiptoAddressId = @currentShipToAddressId;
				END
				ELSE
				BEGIN
					-- Insert a new mapping
					INSERT INTO Erp_ShiptoAddress_Erp_Account_Map (ErpShiptoAddressId, ErpAccountId, ErpShipToAddressCreatedByTypeId)
					VALUES (@currentShipToAddressId, @accountId,10);
				END
			END
		END

		FETCH NEXT FROM shipto_import_cursor INTO 
			@ShipToCode, @ShipToName, @Company, @Country, @StateProvince, @City, 
			@Address1, @Address2, @Suburb, @ZipPostalCode, @PhoneNumber, 
			@DeliveryNotes, @EmailAddresses, @AccountNumber, 
			@AccountSalesOrganisationCode, @IsActive;
	END

	CLOSE shipto_import_cursor;
	DEALLOCATE shipto_import_cursor;

	TRUNCATE TABLE [dbo].ErpShipToAddressImport;
END
