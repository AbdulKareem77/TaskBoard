SELECT t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
       t.CreatedByUserId, t.RowVersion, t.DateCreated, t.DateUpdated,
       ta.UserId AS AssigneeUserId,
       CONCAT(au.FirstName, ' ', au.LastName) AS AssigneeFullName,
       p.Name AS ProjectName
FROM dbo.TaskItems t
JOIN dbo.Projects p ON p.Id = t.ProjectId
LEFT JOIN dbo.TaskAssignments ta ON ta.TaskItemId = t.Id AND ta.UserId = @UserId
LEFT JOIN dbo.Users au ON au.Id = ta.UserId
WHERE t.ProjectId IN @ProjectIds
  AND ta.UserId = @UserId
ORDER BY t.DateCreated DESC
