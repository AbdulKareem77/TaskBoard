INSERT INTO dbo.DomainEventOutbox (Id, EventName, EntityId, EntityType, Payload)
VALUES (@Id, @EventName, @EntityId, @EntityType, @Payload);
