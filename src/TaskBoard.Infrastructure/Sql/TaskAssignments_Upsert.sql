IF NOT EXISTS (SELECT 1 FROM dbo.TaskAssignments WHERE TaskItemId = @TaskItemId AND UserId = @UserId)
BEGIN
    INSERT INTO dbo.TaskAssignments (TaskItemId, UserId)
    VALUES (@TaskItemId, @UserId);
END
