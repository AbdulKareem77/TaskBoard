INSERT INTO dbo.Projects (Id, Name, Description, OwnerId)
VALUES (@Id, @Name, @Description, @OwnerId);
SELECT @Id;
