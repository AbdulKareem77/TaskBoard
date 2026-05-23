INSERT INTO dbo.TaskItems (Id, ProjectId, Title, Description, Status, Priority, DueDate, CreatedByUserId)
VALUES (@Id, @ProjectId, @Title, @Description, @Status, @Priority, @DueDate, @CreatedByUserId);
