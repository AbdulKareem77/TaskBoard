INSERT INTO dbo.UserNotifications (Id, UserId, Type, Title, Message, ReferenceId)
VALUES (@Id, @UserId, @Type, @Title, @Message, @ReferenceId);
SELECT @Id;
