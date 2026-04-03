using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.Infrastructure.Auth;
using TaskBoard.Infrastructure.Cache;
using TaskBoard.Infrastructure.Events;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TaskBoard")
            ?? throw new InvalidOperationException("TaskBoard connection string is required.");

        // Connection factory
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

        // Repositories
        services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
        services.AddScoped<IProjectRepository>(_ => new ProjectRepository(connectionString));
        services.AddScoped<ITaskRepository>(_ => new TaskRepository(connectionString));
        services.AddScoped<INotificationRepository>(_ => new NotificationRepository(connectionString));
        services.AddScoped<IDomainEventOutboxRepository>(_ => new DomainEventOutboxRepository(connectionString));

        // Cache/Redis
        services.AddSingleton<IInMemoryRedis, InMemoryRedis>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ISessionStore, SessionStore>();
        services.AddScoped<IQueueService, QueueService>();

        // Auth
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Events
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        return services;
    }
}
