MERGE dbo.ProjectMembers AS target
USING (SELECT @ProjectId AS ProjectId, @UserId AS UserId) AS source
ON target.ProjectId = source.ProjectId AND target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET Role = @Role
WHEN NOT MATCHED THEN
    INSERT (ProjectId, UserId, Role) VALUES (@ProjectId, @UserId, @Role);
