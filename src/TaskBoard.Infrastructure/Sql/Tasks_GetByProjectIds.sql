SELECT t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate,
       t.CreatedByUserId, t.RowVersion, t.DateCreated, t.DateUpdated,
       ta.UserId AS AssigneeUserId,
       CONCAT(au.FirstName, ' ', au.LastName) AS AssigneeFullName,
       p.Name AS ProjectName,
       th.Id AS HistoryId, th.Action, th.OldValue, th.NewValue, th.DateCreated AS HistoryDate,
       CONCAT(hu.FirstName, ' ', hu.LastName) AS HistoryUserName
FROM dbo.TaskItems t
JOIN dbo.Projects p ON p.Id = t.ProjectId
LEFT JOIN dbo.TaskAssignments ta ON ta.TaskItemId = t.Id
LEFT JOIN dbo.Users au ON au.Id = ta.UserId
LEFT JOIN dbo.TaskHistory th ON th.TaskItemId = t.Id
LEFT JOIN dbo.Users hu ON hu.Id = th.UserId
WHERE t.ProjectId IN @ProjectIds
ORDER BY th.DateCreated DESC
