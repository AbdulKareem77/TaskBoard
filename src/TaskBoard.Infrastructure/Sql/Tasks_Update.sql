UPDATE dbo.TaskItems
SET Title = @Title,
    Description = @Description,
    Status = @Status,
    Priority = @Priority,
    DueDate = @DueDate,
    RowVersion = RowVersion + 1,
    DateUpdated = SYSUTCDATETIME()
WHERE Id = @Id AND RowVersion = @ExpectedVersion;
SELECT @@ROWCOUNT AS RowsAffected;
