SELECT pm.Id, pm.ProjectId, pm.UserId, pm.Role, pm.DateAdded,
       u.FirstName, u.LastName, u.Email
FROM dbo.ProjectMembers pm
JOIN dbo.Users u ON u.Id = pm.UserId
WHERE pm.ProjectId = @ProjectId
ORDER BY u.FirstName
