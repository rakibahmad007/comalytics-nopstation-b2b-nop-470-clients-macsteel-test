CREATE PROCEDURE [dbo].[CP_DomainImportProcedure]
AS
BEGIN

-- Declare the variables to store the values returned by FETCH.  
DECLARE @DomainOrEmailName nvarchar(50),
	@TypeId nvarchar(100),
	@IsActive nvarchar (100);

	-- cursor for import domain
	DECLARE domain_import_cursor CURSOR FAST_FORWARD FOR  
	SELECT [DomainOrEmailName]
			,[TypeId]
			,[IsActive] FROM [dbo].[CP_DomainImport];

	OPEN domain_import_cursor;

	-- Perform the first fetch
	FETCH NEXT FROM domain_import_cursor INTO @DomainOrEmailName, @TypeId, @IsActive;

	-- Check @@FETCH_STATUS to see if there are any more rows to fetch.  
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @currentDomainId int = 0
		SET @currentDomainId = (SELECT Top(1) Id FROM [dbo].[CP_Domain] domain with (NOLOCK) where domain.DomainOrEmailName = @DomainOrEmailName);

		-- if Domain is not exist we will add those otherwise we will update
		IF @currentDomainId IS NULL OR @currentDomainId < 1
		-- Domain is not exist begin
		BEGIN
			INSERT INTO [dbo].[CP_Domain]
			([DomainOrEmailName]
			,[TypeId]
			,[IsActive])
			VALUES
				(@DomainOrEmailName
				,CASE
					When @TypeId = '5' OR @TypeId = 'DOMAIN' OR @TypeId = 'Domain' OR @TypeId = 'domain' THEN 5
					ELSE 10
				END
				,CASE
					When @IsActive = '1' OR @IsActive = 'True' OR @IsActive = 'TRUE' OR @IsActive = 'true' THEN 1
					ELSE 0
				END)
		END -- Domain is not exist end
		ELSE
		-- Domain is exist begin
		BEGIN
			UPDATE [dbo].[CP_Domain]
				SET [DomainOrEmailName] = @DomainOrEmailName
					,[TypeId] =
					CASE
						When @TypeId = '5' OR @TypeId = 'DOMAIN' OR @TypeId = 'Domain' OR @TypeId = 'domain' THEN 5
						ELSE 10
					END
					,[IsActive] = 
					CASE
						When @IsActive = '1' OR @IsActive = 'True' OR @IsActive = 'TRUE' OR @IsActive = 'true' THEN 1
						ELSE 0
					END
				WHERE Id = @currentDomainId
		END -- Domain is exist end

		-- Perform the next fetch
		FETCH NEXT FROM domain_import_cursor INTO @DomainOrEmailName, @TypeId, @IsActive;
	END
		
	CLOSE domain_import_cursor;  
	DEALLOCATE domain_import_cursor; 

	TRUNCATE TABLE [dbo].[CP_DomainImport];
END
GO