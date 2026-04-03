SELECT u.Id, u.Email, u.PasswordHash, u.FirstName, u.LastName, u.IsActive, u.DateCreated, u.DateUpdated,
       r.Id AS RoleId, r.Name AS RoleName
FROM dbo.Users u
LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.Id
LEFT JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE u.Email = @Email AND u.IsActive = 1
