using System.Security.Cryptography;
using System.Text;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.NotificationConsumer.Services;

public class ChangeDetectionService
{
    private readonly INotificationRepository _notificationRepository;

    public ChangeDetectionService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public string ComputeHash(params string?[] values)
    {
        var combined = string.Join("|", values.Select(v => v ?? string.Empty));
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<bool> HasChangedAsync(Guid entityId, string entityType, string newHash)
    {
        var existingHash = await _notificationRepository.GetNotificationIndexHashAsync(entityId, entityType);
        return existingHash == null || !existingHash.Equals(newHash, StringComparison.OrdinalIgnoreCase);
    }

    public async Task UpdateHashAsync(Guid entityId, string entityType, string hash)
    {
        await _notificationRepository.UpsertNotificationIndexAsync(entityId, entityType, hash);
    }
}
