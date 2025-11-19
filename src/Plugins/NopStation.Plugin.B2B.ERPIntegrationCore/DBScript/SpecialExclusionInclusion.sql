

        /****** Card / branch :  clients/macsteel/feature/2937-macsteel-upgradation-2601-special-include-and-special-exclude    Script Date: 27-December-24 ******/
        /****** Updated SP Name: SpecialExclusionInclusion for special-include-and-special-exclude Admin CRUD operations ******/

        GO

        ALTER PROCEDURE [dbo].[SpecialExclusionInclusion] (@erpAccountId int = 0, @ProductId int = 0,@Type int = 0,@IsActive BIT)
        AS
        BEGIN
	        declare @prefilterSpecAttributeId int = 0;

	        select @prefilterSpecAttributeId = Id
	        from SpecificationAttribute
	        where [Name] like 'PreFilter';
	        IF @prefilterSpecAttributeId > 0
	        BEGIN
		        DECLARE @B2BAccountExlcudeOrInCludeVal table( [SpecialExclusionOrInclusion] varchar(250));
		
		        DECLARE @InsertedOrDeletedSpecification table( [Id] int,
						        [ProductId] int,
                                [SpecificationAttributeOptionId] int,
                                [AllowFiltering] bit,
                                [ShowOnProductPage] bit,
                                [Operation] bit);

		        DECLARE @SpecialExclusionOrInclusion varchar(250);

		        IF @Type = 10
		        BEGIN
			        SELECT @SpecialExclusionOrInclusion = b2b.SpecialIncludes
			        FROM [dbo].Erp_Account b2b WHERE b2b.Id = @erpAccountId
			        IF @SpecialExclusionOrInclusion IS NULL
			        BEGIN
				        UPDATE [dbo].[Erp_Account]
				        SET SpecialIncludes = CAST(@erpAccountId AS varchar) + '_SpIncl'
				        OUTPUT INSERTED.SpecialIncludes INTO @B2BAccountExlcudeOrInCludeVal
				        Where Id = @erpAccountId;

				        SELECT @SpecialExclusionOrInclusion = SpecialExclusionOrInclusion
				        FROM @B2BAccountExlcudeOrInCludeVal;
			        END
		        END
		        ELSE
			        SELECT @SpecialExclusionOrInclusion = b2b.SpecialExcludes
			        FROM [dbo].Erp_Account b2b WHERE b2b.Id = @erpAccountId
			        IF @SpecialExclusionOrInclusion IS NULL
			        BEGIN
				        UPDATE [dbo].[Erp_Account]
				        SET SpecialExcludes = CAST(@erpAccountId AS varchar) + '_SpExcl'
				        OUTPUT INSERTED.SpecialExcludes INTO @B2BAccountExlcudeOrInCludeVal
				        Where Id = @erpAccountId;

				        SELECT @SpecialExclusionOrInclusion = SpecialExclusionOrInclusion
				        FROM @B2BAccountExlcudeOrInCludeVal
			        END

		        IF @SpecialExclusionOrInclusion IS NOT NULL
		        BEGIN
			
			        IF @Type = 10 AND @IsActive = 1
			        BEGIN
				        --Delete existing prefilter facets without special inclusion
				        DELETE Product_SpecificationAttribute_Mapping WHERE ProductId = @ProductId AND SpecificationAttributeOptionId IN (SELECT Id FROM [dbo].[SpecificationAttributeOption] Where SpecificationAttributeId = @prefilterSpecAttributeId AND Name NOT LIKE ('%Incl%'))
                    END

			        DECLARE @Code varchar(250);
			        DECLARE All_CODE CURSOR FAST_FORWARD FOR
			        SELECT data FROM [nop_splitstring_to_table](@SpecialExclusionOrInclusion, ',')

			        OPEN All_CODE;
			        FETCH NEXT FROM All_CODE
			        INTO @Code;
			        WHILE @@FETCH_STATUS = 0 
			        BEGIN
				        DECLARE @specAttrOptionId int = 0;
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
				        SELECT @SpecificationAttributeMappingId = Id FROM Product_SpecificationAttribute_Mapping with (NOLOCK) WHERE ProductId = @ProductId AND SpecificationAttributeOptionId = @specAttrOptionId;
				 
				        -- check if same mapping exist or not (we will only add if not exist)
				        IF (@SpecificationAttributeMappingId = 0 OR @SpecificationAttributeMappingId IS NULL) AND @specAttrOptionId > 0
				        BEGIN
					        IF @IsActive = 1
					        BEGIN
						        INSERT INTO Product_SpecificationAttribute_Mapping (ProductId, AttributeTypeId, SpecificationAttributeOptionId, AllowFiltering, ShowOnProductPage, DisplayOrder)
						        OUTPUT inserted.Id, inserted.ProductId, inserted.SpecificationAttributeOptionId,inserted.AllowFiltering, inserted.ShowOnProductPage,1 INTO @InsertedOrDeletedSpecification
						        VALUES (@ProductId,0, @specAttrOptionId, 0, 0, 0);
					        END
				        END
				        ELSE
					        IF @IsActive = 0
					        BEGIN
						        DELETE FROM Product_SpecificationAttribute_Mapping 
						        OUTPUT deleted.Id, deleted.ProductId, deleted.SpecificationAttributeOptionId,deleted.AllowFiltering, deleted.ShowOnProductPage,0 INTO @InsertedOrDeletedSpecification
						        WHERE Id = @SpecificationAttributeMappingId;

                                IF (NOT EXISTS(SELECT * FROM [dbo].[Erp_Special_Includes_And_Excludes] WHERE [ErpAccountId] = @erpAccountId) 
                                    OR NOT EXISTS(SELECT * FROM [dbo].[Erp_Special_Includes_And_Excludes] WHERE [ErpAccountId] = @erpAccountId AND [IsActive] = 1))
                                BEGIN
                                IF(@Type = 10)
                                BEGIN 
                                    UPDATE [dbo].[Erp_Account]
				                    SET SpecialIncludes = NULL
				                    Where Id = @erpAccountId;
                                END
                                ELSE
                                BEGIN
                                    UPDATE [dbo].[Erp_Account]
				                    SET SpecialExcludes = NULL
				                    Where Id = @erpAccountId;
                                END
                        
                                END

					        END
				        FETCH NEXT FROM All_CODE
				        INTO @Code;
			        END
			        CLOSE All_CODE;
			        DEALLOCATE All_CODE;
			        SELECT * FROM @InsertedOrDeletedSpecification;
		        END
	        END
        END


        /****** Card / branch :  clients/macsteel/feature/2937-macsteel-upgradation-2601-special-include-and-special-exclude    Script Date: 27-December-24 ******/
        /****** Updated SP Name: SP_B2BCustomerAccount_UpdateOrDeleteSpecialIncludeExcludes for special-include-and-special-exclude Admin CRUD operations ******/

        GO

        ALTER PROCEDURE [dbo].[SP_B2BCustomerAccount_UpdateOrDeleteSpecialIncludeExcludes]
            @ids AS [dbo].[B2BCustomerAccount_SpecialIncludeExcludeIdType] READONLY,
            @mode AS tinyint,
            @active bit
        AS   
	        BEGIN
		        DECLARE @prodSpecAttrMappings AS TABLE([Id] int,
						        [ProductId] int,
                                [SpecificationAttributeOptionId] int,
                                [AllowFiltering] bit,
                                [ShowOnProductPage] bit,
                                [Operation] bit);
		        --inner cursor
                DECLARE @id AS int;
		        DECLARE @ErpAccountId AS int;
		        DECLARE @SalesOrgId AS int;
		        DECLARE @ProductId AS int;
		        DECLARE @SpecialTypeId AS int;
		        DECLARE @IsActive AS bit;
		        DECLARE @LastUpdate AS datetime2;
		        DECLARE idCursor CURSOR LOCAL FAST_FORWARD FOR
			        SELECT * FROM @ids
		        OPEN idCursor
		        FETCH NEXT FROM idCursor INTO @id
		        WHILE @@FETCH_STATUS = 0 
			        BEGIN
				        -- do your tasks here
                        SELECT @id = [Id], @ErpAccountId = [ErpAccountId], @SalesOrgId = [ErpSalesOrgId], @ProductId = [ProductId], @SpecialTypeId = [SpecialTypeId],
                        @IsActive = [IsActive]  FROM [dbo].[Erp_Special_Includes_And_Excludes] WHERE [Id] = @id;
                        IF(@id IS NOT NULL)
                        BEGIN
                            IF(@mode = 1)
                            BEGIN
                                DELETE FROM [dbo].[Erp_Special_Includes_And_Excludes] WHERE [Id] = @id;
                                -- execute sp for create and get affected Product_SpecificationAttribute_Mapping table rows
					            INSERT INTO @prodSpecAttrMappings  EXEC [dbo].[SpecialExclusionInclusion] @erpAccountId = @ErpAccountId, @ProductId = @ProductId, @Type = @SpecialTypeId, @IsActive = 0;
                            END
                            ELSE
                            BEGIN
                                 IF(@mode = 0)
                                 BEGIN
                                    UPDATE [dbo].[Erp_Special_Includes_And_Excludes] SET [IsActive] = @active WHERE [Id] = @id;
                                 END
                             -- execute sp for affected Product_SpecificationAttribute_Mapping table rows
                             INSERT INTO @prodSpecAttrMappings  EXEC [dbo].[SpecialExclusionInclusion] @erpAccountId = @ErpAccountId, @ProductId = @ProductId, @Type = @SpecialTypeId, @IsActive = @active;
                            END
                        END
				        FETCH NEXT FROM idCursor INTO @id
			        END
		        CLOSE idCursor
		        DEALLOCATE idCursor
		        SELECT * FROM  @prodSpecAttrMappings;
	        END


            /****** Card / branch :  clients/macsteel/feature/2937-macsteel-upgradation-2601-special-include-and-special-exclude    Script Date: 27-December-24 ******/
            /****** Updated SP Name: SP_B2BCustomerAccount_ImportSpecialIncludeExcludes for special-include-and-special-exclude Admin CRUD operations ******/

            GO
            
            ALTER PROCEDURE [dbo].[SP_B2BCustomerAccount_ImportSpecialIncludeExcludes]
            @data AS [dbo].[B2BCustomerAccount_SpecialIncludeExcludeImportType] READONLY,
            @type int
        AS   
	        BEGIN
		        DECLARE @tempSpecialExcludeIncludeTable TABLE(
			        [ErpAccountId] [int] NOT NULL,
			        [SalesOrgId] [int] NOT NULL,
			        [ProductId] [int] NOT NULL,
			        [SpecialTypeId] [int] NOT NULL,
			        [IsActive] [bit] NOT NULL,
			        [LastUpdate] [datetime2](7) NOT NULL
		        );
                --common
                DECLARE @existingOrNotFound AS bit;
                DECLARE @recordsWithFailedInsert AS TABLE (
                    [AccountNumber] nvarchar(50) NOT NULL,
	                [SalesOrgCode] nvarchar(10) NOT NULL,
	                [SKU] nvarchar(400) NOT NULL,
	                [IsActive] bit NOT NULL);
		        DECLARE @prodSpecAttrMappings AS TABLE([Id] int,
						        [ProductId] int,
                                [SpecificationAttributeOptionId] int,
                                [AllowFiltering] bit,
                                [ShowOnProductPage] bit,
                                [Operation] bit);
		        --outer cursor
		        DECLARE @dataRowAccNo AS nvarchar(50);
		        DECLARE @dataRowSalesOrgCode AS nvarchar(10);
		        DECLARE @dataRowSKU AS nvarchar(400);
		        DECLARE @dataRowIsActive AS bit;
		        --inner cursor
		        DECLARE @ErpAccountId AS int;
		        DECLARE @SalesOrgId AS int;
		        DECLARE @ProductId AS int;
		        DECLARE @SpecialTypeId AS int;
		        DECLARE @IsActive AS bit;
		        DECLARE @LastUpdate AS datetime2;
		        DECLARE dataCursor CURSOR LOCAL FAST_FORWARD FOR
			        SELECT * FROM @data
		        OPEN dataCursor
		        FETCH NEXT FROM dataCursor INTO @dataRowAccNo, @dataRowSalesOrgCode, @dataRowSKU, @dataRowIsActive
		        WHILE @@FETCH_STATUS = 0 
			        BEGIN
				        -- do your tasks here
                        SET @existingOrNotFound = 0;
				        INSERT INTO @tempSpecialExcludeIncludeTable ([ErpAccountId], [SalesOrgId], [ProductId], [SpecialTypeId], [IsActive], [LastUpdate])
				        SELECT DISTINCT [Erp_Account].[Id] [ErpAccountId], SalesOrg.[Id] [SalesOrgId], [Prod].[Id] [ProductId], @type [SpecialTypeId], @dataRowIsActive [IsActive], GETUTCDATE() [LastUpdate] FROM [dbo].[Erp_Account] INNER JOIN [dbo].[Erp_Sales_Org] SalesOrg 
				        ON [Erp_Account].[ErpSalesOrgId] = SalesOrg.[Id], [dbo].[Product] [Prod] WHERE [Prod].[Sku] = @dataRowSKU AND [Erp_Account].[AccountNumber] = @dataRowAccNo
				        AND SalesOrg.Code = @dataRowSalesOrgCode
				        AND [Erp_Account].[IsDeleted] = 0 AND SalesOrg.[IsDeleted] = 0 AND [Prod].[Deleted] = 0
				        GROUP BY [Erp_Account].[Id], SalesOrg.[Id], [Prod].[Id];

				        --loop through this table variable. If no entry exists then insert, otherwise update
				        DECLARE innerDataCursor CURSOR LOCAL FAST_FORWARD FOR
				        SELECT * FROM @tempSpecialExcludeIncludeTable
				        OPEN innerDataCursor
				        FETCH NEXT FROM innerDataCursor INTO @ErpAccountId, @SalesOrgId, @ProductId, @SpecialTypeId, @IsActive, @LastUpdate
				        WHILE @@FETCH_STATUS = 0 
				        BEGIN
					        IF(NOT EXISTS(SELECT * FROM [dbo].[Erp_Special_Includes_And_Excludes] WHERE [ErpAccountId] = @ErpAccountId 
					        AND [ErpSalesOrgId] = @SalesOrgId AND [ProductId] = @ProductId))
						        BEGIN
							        INSERT INTO [dbo].[Erp_Special_Includes_And_Excludes] ([ErpAccountId], [ErpSalesOrgId], [ProductId], [SpecialTypeId], [IsActive], [LastUpdate])
							        SELECT @ErpAccountId, @SalesOrgId, @ProductId, @SpecialTypeId, @IsActive, @LastUpdate;

							        -- execute sp for create and get affected Product_SpecificationAttribute_Mapping table rows
							        INSERT INTO @prodSpecAttrMappings  EXEC [dbo].[SpecialExclusionInclusion] @b2bAccountId = @ErpAccountId, @ProductId = @ProductId, @Type = @SpecialTypeId, @IsActive = @IsActive;
						        END
					        ELSE
						        BEGIN
                                     -- having one matching record is sufficient to mark as existing
							         SET @existingOrNotFound = 1;
						        END
					        FETCH NEXT FROM innerDataCursor INTO @ErpAccountId, @SalesOrgId, @ProductId, @SpecialTypeId, @IsActive, @LastUpdate
				        END
				        CLOSE innerDataCursor
				        DEALLOCATE innerDataCursor
				        DELETE FROM @tempSpecialExcludeIncludeTable;
                        IF(@existingOrNotFound = 1)
                        BEGIN
                            INSERT INTO @recordsWithFailedInsert([AccountNumber], [SalesOrgCode], [SKU], [IsActive]) VALUES(@dataRowAccNo, @dataRowSalesOrgCode, @dataRowSKU, @dataRowIsActive);
                        END

				        FETCH NEXT FROM dataCursor INTO @dataRowAccNo, @dataRowSalesOrgCode, @dataRowSKU, @dataRowIsActive
			        END
		        CLOSE dataCursor
		        DEALLOCATE dataCursor
                SELECT * FROM @recordsWithFailedInsert;
		        SELECT * FROM  @prodSpecAttrMappings;
	        END 
