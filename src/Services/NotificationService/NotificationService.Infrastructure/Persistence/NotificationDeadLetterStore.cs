using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDeadLetterStore
{
    private const int MaxRetryCount = 10;
    private readonly NotificationDbContext _dbContext;

    public NotificationDeadLetterStore(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> RecordFailureAsync(string routingKey, string payload, string error, CancellationToken cancellationToken)
    {
        var snapshot = ExtractSnapshot(payload);
        var eventType = snapshot.EventType == "Unknown" ? routingKey : snapshot.EventType;
        var deadLetter = await _dbContext.NotificationDeadLetters
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.EventId == snapshot.EventId, cancellationToken);

        if (deadLetter is null)
        {
            deadLetter = new NotificationDeadLetter
            {
                EventId = snapshot.EventId,
                TenantId = snapshot.TenantId,
                ExpenseId = snapshot.ExpenseId,
                EventType = eventType,
                RoutingKey = routingKey,
                CorrelationId = snapshot.CorrelationId,
                Payload = payload,
                Error = error,
                RetryCount = 0
            };

            await _dbContext.NotificationDeadLetters.AddAsync(deadLetter, cancellationToken);
        }

        deadLetter.RetryCount += 1;
        deadLetter.Error = error.Length > 2000 ? error[..2000] : error;
        if (deadLetter.RetryCount >= MaxRetryCount)
        {
            deadLetter.DeadLetteredAt ??= DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return deadLetter.RetryCount;
    }

    public static bool ShouldDeadLetter(int retryCount)
    {
        return retryCount >= MaxRetryCount;
    }

    private static EventSnapshot ExtractSnapshot(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            return new EventSnapshot(
                GetGuid(root, "EventId") ?? Guid.NewGuid(),
                GetGuid(root, "TenantId"),
                GetGuid(root, "ExpenseId"),
                GetString(root, "EventType") ?? "Unknown",
                GetString(root, "CorrelationId") ?? string.Empty);
        }
        catch (JsonException)
        {
            return new EventSnapshot(Guid.NewGuid(), null, null, "Unknown", string.Empty);
        }
    }

    private static Guid? GetGuid(JsonElement root, string propertyName)
    {
        if (TryGetProperty(root, propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            Guid.TryParse(property.GetString(), out var value))
        {
            return value;
        }

        return null;
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        if (TryGetProperty(root, propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement root, string propertyName, out JsonElement property)
    {
        return root.TryGetProperty(propertyName, out property) ||
               root.TryGetProperty(char.ToLowerInvariant(propertyName[0]) + propertyName[1..], out property);
    }

    private sealed record EventSnapshot(Guid EventId, Guid? TenantId, Guid? ExpenseId, string EventType, string CorrelationId);
}
