-- Returns "TRUE", if finds a personal chat of user with himself.
-- Returns "FALSE", if such chat was not found.

DECLARE @Enumerator TABLE (id uniqueidentifier)

INSERT INTO @Enumerator
SELECT [ChatId]
FROM [ChatMembers]
WHERE [UserId] = @user 

DECLARE @id uniqueidentifier
DECLARE @numOfMembers int

WHILE EXISTS(SELECT 1 FROM @Enumerator)
BEGIN
     SELECT TOP 1 @id = id FROM @Enumerator 

	 SELECT @numOfMembers = COUNT([UserId]) 
	 FROM [ChatMembers]
	 WHERE [ChatId] = @id

	 IF (@numOfMembers = 1) 
	 BEGIN
		SELECT 'TRUE' AS [RESULT]
		RETURN
	 END

     DELETE FROM @Enumerator WHERE id = @id
END

SELECT 'FALSE' AS [RESULT]
