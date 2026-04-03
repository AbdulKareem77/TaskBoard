SELECT t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
       t.CreatedByUserId, t.RowVersion, t.DateCreated, t.DateUpdated,
       ta.UserId AS AssigneeUserId,
       CONCAT(u.FirstName, ' ', u.LastName) AS AssigneeFullName,
       COUNT(*) OVER() AS TotalCount
FROM dbo.TaskItems t
LEFT JOIN dbo.TaskAssignments ta ON ta.TaskItemId = t.Id
LEFT JOIN dbo.Users u ON u.Id = ta.UserId
WHERE t.ProjectId = @ProjectId
  AND (@Status IS NULL OR t.Status = @Status)
ORDER BY t.DateCreated DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
