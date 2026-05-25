SELECT c.Id, c.TaskItemId, c.UserId, c.Content, c.DateCreated,
       CONCAT(u.FirstName, ' ', u.LastName) AS AuthorFullName
FROM dbo.TaskComments c
JOIN dbo.Users u ON u.Id = c.UserId
WHERE c.TaskItemId = @TaskItemId
ORDER BY c.DateCreated DESC;
