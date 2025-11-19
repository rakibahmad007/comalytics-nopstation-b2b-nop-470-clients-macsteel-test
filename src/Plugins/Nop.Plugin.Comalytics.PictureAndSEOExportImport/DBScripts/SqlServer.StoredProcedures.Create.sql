
CREATE PROCEDURE [dbo].[ProductPictureAndSEOUpdateExcelImportProcedure]
AS
BEGIN
-- current UTCDateTime
DECLARE @CurrentUTCDATE datetime = GETUTCDATE();
-- Declare the variables to store the values returned by FETCH.  
DECLARE @ProductId int,
    @Name nvarchar (400),
    @Sku nvarchar (400),
    @MetaKeywords nvarchar (400),
    @MetaDescription nvarchar (400),
    @MetaTitle nvarchar (400),
    @SeName nvarchar (400),
    @Picture1Id int,
    @Picture1Alt nvarchar (400),
    @Picture1Title nvarchar (400),
    @Picture2Id int,
    @Picture2Alt nvarchar (400),
    @Picture2Title nvarchar (400),
    @Picture3Id int,
    @Picture3Alt nvarchar (400),
    @Picture3Title nvarchar (400);

	-- cursor for import user
	DECLARE product_picture_and_seo_update_excel_import_cursor CURSOR FAST_FORWARD FOR  
	SELECT  [ProductId]
			,[Name]
            ,[Sku]
            ,[MetaKeywords]
            ,[MetaDescription]
            ,[MetaTitle]
            ,[SeName]
            ,[Picture1Id]
            ,[Picture1Alt]
            ,[Picture1Title]
            ,[Picture2Id]
            ,[Picture2Alt]
            ,[Picture2Title]
            ,[Picture3Id]
            ,[Picture3Alt]
            ,[Picture3Title]
		FROM [dbo].[ProductPictureAndSEOUpdateExcelImport];

        Declare @errors nvarchar(MAX);
	    Set @errors = '';

	OPEN product_picture_and_seo_update_excel_import_cursor;

	-- Perform the first fetch
	FETCH NEXT FROM product_picture_and_seo_update_excel_import_cursor INTO  @ProductId, @Name, @Sku, @MetaKeywords, @MetaDescription, @MetaTitle, @SeName, @Picture1Id, @Picture1Alt,@Picture1Title, @Picture2Id, @Picture2Alt, @Picture2Title, @Picture3Id, @Picture3Alt, @Picture3Title;

	-- Check @@FETCH_STATUS to see if there are any more rows to fetch.  
		WHILE @@FETCH_STATUS = 0
	BEGIN

		Declare @temp nvarchar(MAX),
				@error nvarchar(MAX);
		Set @temp = ''
		Set @error = ''

		IF @ProductId > 0 
		    BEGIN
                BEGIN TRY
                    IF EXISTS (SELECT TOP(1) * FROM [dbo].[Product] WHERE Id = @ProductId)
			            BEGIN

							Update [dbo].[Product]
							set
							Name = @Name,
							MetaKeywords=@MetaKeywords,
                            MetaDescription = @MetaDescription,
                            MetaTitle = @MetaTitle
							where Id = @ProductId

							Update [dbo].[UrlRecord]
							set
							Slug = @SeName
							Where EntityName = 'Product' AND EntityId = @ProductId

							IF @Picture1Id > 0
								BEGIN
									Update [dbo].[Picture]
									Set
									AltAttribute = @Picture1Alt,
									TitleAttribute = @Picture1Title
									Where Id = @Picture1Id
									IF @@ROWCOUNT = 0
									BEGIN
										Set @temp = CONCAT('    ', 'Picture does not exist with Id ', @Picture1Id, CHAR(10));
									Set @error = CONCAT(@error, @temp)
									END
								END

							IF @Picture2Id > 0
								BEGIN
									Update [dbo].[Picture]
									Set
									AltAttribute = @Picture2Alt,
									TitleAttribute = @Picture2Title
									Where Id = @Picture2Id
									IF @@ROWCOUNT = 0
									BEGIN
										Set @temp = CONCAT('    ', 'Picture does not exist with Id ', @Picture2Id, CHAR(10));
									Set @error = CONCAT(@error, @temp)
									END
								END
								

							IF @Picture3Id > 0
								BEGIN
									Update [dbo].[Picture]
									Set
									AltAttribute = @Picture3Alt,
									TitleAttribute = @Picture3Title
									Where Id = @Picture3Id
									IF @@ROWCOUNT = 0
									BEGIN
										Set @temp = CONCAT('    ', 'Picture does not exist with Id ', @Picture3Id, CHAR(10));
									Set @error = CONCAT(@error, @temp)
									END
								END
								
			            END

					ELSE
						BEGIN
							Set @temp = CONCAT('    ', 'Product does not exist with Id ', @ProductId, CHAR(10));
							Set @error = CONCAT(@error, @temp)
						END
                END TRY

                BEGIN CATCH
                    DECLARE @ErrorMessage nvarchar(max) = isnull(ERROR_MESSAGE(), '')
					Set @temp = CONCAT('    ', @ErrorMessage, CHAR(10));
					Set @error = CONCAT(@error, @temp)
                END CATCH
		    END

		IF LEN(@error) > 0
		BEGIN
			Set @errors = CONCAT(@errors, 'Product Id ', @ProductId,CHAR(10), @error);
		END

        FETCH NEXT FROM product_picture_and_seo_update_excel_import_cursor INTO  @ProductId, @Name, @Sku, @MetaKeywords, @MetaDescription, @MetaTitle, @SeName, @Picture1Id, @Picture1Alt,@Picture1Title, @Picture2Id, @Picture2Alt, @Picture2Title, @Picture3Id, @Picture3Alt, @Picture3Title;
	END
		
	CLOSE product_picture_and_seo_update_excel_import_cursor;  
	DEALLOCATE product_picture_and_seo_update_excel_import_cursor; 

	TRUNCATE TABLE [dbo].[ProductPictureAndSEOUpdateExcelImport];

	IF LEN(@errors) > 0
		BEGIN
			INSERT INTO [dbo].[Log]
						([LogLevelId],
						[ShortMessage],
						[FullMessage],
						[CreatedOnUtc])
						VALUES
							(40
							,'Issues in updating product picture attributes and SEO fields'
							,@errors
							,@CurrentUTCDATE)
			return @@IDENTITY
		END

END