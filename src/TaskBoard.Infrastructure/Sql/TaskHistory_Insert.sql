INSERT INTO dbo.TaskHistory (Id, TaskItemId, UserId, Action, OldValue, NewValue)
VALUES (@Id, @TaskItemId, @UserId, @Action, @OldValue, @NewValue);
