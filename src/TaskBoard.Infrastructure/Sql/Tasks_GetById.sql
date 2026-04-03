SELECT t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
       t.CreatedByUserId, t.RowVersion, t.DateCreated, t.DateUpdated,
       CONCAT(cu.FirstName, ' ', cu.LastName) AS CreatedByFullName,
       ta.UserId AS AssigneeUserId,
       CONCAT(au.FirstName, ' ', au.LastName) AS AssigneeFullName,
       th.Id AS HistoryId, th.Action AS HistoryAction, th.OldValue, th.NewValue, th.DateCreated AS HistoryDate,
       CONCAT(hu.FirstName, ' ', hu.LastName) AS HistoryUserName
FROM dbo.TaskItems t
LEFT JOIN dbo.Users cu ON cu.Id = t.CreatedByUserId
LEFT JOIN dbo.TaskAssignments ta ON ta.TaskItemId = t.Id
LEFT JOIN dbo.Users au ON au.Id = ta.UserId
LEFT JOIN dbo.TaskHistory th ON th.TaskItemId = t.Id
LEFT JOIN dbo.Users hu ON hu.Id = th.UserId
WHERE t.Id = @TaskId
ORDER BY th.DateCreated DESC
