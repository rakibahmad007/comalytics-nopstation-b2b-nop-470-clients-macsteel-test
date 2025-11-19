CREATE PROCEDURE [dbo].[UpdateOrderFromParallelTableErpOrder]
AS
BEGIN
    SELECT * FROM [dbo].[ErpOrder];
END;