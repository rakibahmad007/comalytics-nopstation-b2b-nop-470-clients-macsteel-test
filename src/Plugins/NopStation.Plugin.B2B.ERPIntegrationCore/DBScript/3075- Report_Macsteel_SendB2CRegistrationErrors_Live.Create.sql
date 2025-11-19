CREATE OR ALTER PROCEDURE [dbo].[Report_Macsteel_SendB2CRegistrationErrors_Live]
    AS
    BEGIN
        DECLARE @EmailBody NVARCHAR(MAX);
        DECLARE @ErrorCount INT;
        DECLARE @Subject NVARCHAR(200);

        -- Query to get the required information
        SELECT DISTINCT C.Email, B.ErrorMessage
        INTO #TempErrors
        FROM [dbo].[Customer_CustomerRole_Mapping] CCR
        JOIN [dbo].[Customer] C ON CCR.Customer_Id = C.Id
        LEFT JOIN [dbo].[Erp_User_Registration_Info] B ON C.Id = B.NopCustomerId
        WHERE (CCR.CustomerRole_Id = 30 AND C.Deleted = 0)
           OR (CCR.CustomerRole_Id = 3 AND C.Deleted = 0 AND B.ErrorMessage IS NOT NULL)
           AND NOT EXISTS (
               SELECT 1 
               FROM [dbo].[Customer_CustomerRole_Mapping] CCR2
               WHERE CCR2.Customer_Id = C.Id
               AND (CCR2.CustomerRole_Id = 31 OR CCR2.CustomerRole_Id = 29) -- Exclude users with CustomerRole_Id = 29
           );

        -- Check if there are any errors to report
        SET @ErrorCount = (SELECT COUNT(*) FROM #TempErrors);

        IF @ErrorCount > 0
        BEGIN
            -- Create email body with formatted columns and spaces
            SET @EmailBody = N'<html><body><h2>Macsteel: B2C user registration errors - LIVE</h2>';
            SET @EmailBody = @EmailBody + N'<p>There are ' + CAST(@ErrorCount AS NVARCHAR) + N' B2C user registration errors in LIVE.</p>';
            SET @EmailBody = @EmailBody + N'<table border="1" cellpadding="5" cellspacing="0"><tr><th>Email</th><th>Error Message</th></tr>';

            SELECT @EmailBody = @EmailBody + N'<tr><td>' + ISNULL(Email, '') + N'</td><td>' + ISNULL(ErrorMessage, '') + N'</td></tr>'
            FROM #TempErrors;

            SET @EmailBody = @EmailBody + N'</table></body></html>';

            -- Construct the email subject
            SET @Subject = N'Macsteel: B2C registration errors - LIVE (' + CAST(@ErrorCount AS NVARCHAR) + N')';

            -- Send email using Database Mail
            EXEC msdb.dbo.sp_send_dbmail
                @profile_name = 'ecommerce',
                @recipients = 'ecommerce@macsteel.co.za;nicole.watson@macsteel.co.za;fritzferreira@comalytics.com;janniedutoit@comalytics.com',
                @subject = @Subject,
                @body = @EmailBody,
                @body_format = 'HTML';
        END

        -- Drop the temporary table if it exists
        IF OBJECT_ID('tempdb..#TempErrors') IS NOT NULL
            DROP TABLE #TempErrors;
    END;
    
GO
