SELECT p.Id, p.Name, p.Description, p.OwnerId, p.IsArchived, p.DateCreated, p.DateUpdated,
       CONCAT(u.FirstName, ' ', u.LastName) AS OwnerName,
       (SELECT COUNT(*) FROM dbo.ProjectMembers pm2 WHERE pm2.ProjectId = p.Id) AS MemberCount,
       (SELECT COUNT(*) FROM dbo.TaskItems t2 WHERE t2.ProjectId = p.Id) AS TaskCount
FROM dbo.Projects p
JOIN dbo.Users u ON u.Id = p.OwnerId
WHERE (@IsAdmin = 1 OR EXISTS (SELECT 1 FROM dbo.ProjectMembers pm WHERE pm.ProjectId = p.Id AND pm.UserId = @UserId))
  AND p.IsArchived = 0
ORDER BY p.DateCreated DESC
