# Migration ve Seed Bilgileri

## Migration Komutları

Expense DB migration uygulama:

```bash
dotnet ef database update --project src/Services/ExpenseService/ExpenseService.Infrastructure/ExpenseService.Infrastructure.csproj --startup-project src/Services/ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj
```

Notification DB migration uygulama:

```bash
dotnet ef database update --project src/Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj --startup-project src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj
```

Docker Compose ile çalıştırıldığında API containerları migrationları startup sırasında otomatik uygular.

## Expense DB Tabloları

- `Tenants`
- `Users`
- `UserRoles`
- `Expenses`
- `ExpenseApprovals`
- `OutboxMessages`
- `__EFMigrationsHistory`

## Notification DB Tabloları

- `Notifications`
- `ProcessedMessages`
- `__EFMigrationsHistory`

## Seed Tenantlar

| Tenant | Açıklama |
| --- | --- |
| `acme` | Birinci test şirketi |
| `globex` | İkinci test şirketi |

## Seed Kullanıcılar

Tüm kullanıcılar için şifre: `Pass123!`

| Tenant | E-posta | Roller |
| --- | --- | --- |
| `acme` | `admin@acme.com` | Admin |
| `acme` | `hr@acme.com` | HR |
| `acme` | `personel@demo.com` | Personnel |
| `globex` | `admin@globex.com` | Admin |
| `globex` | `hr@globex.com` | HR |
| `globex` | `personel@demo.com` | Personnel |

## E-posta Benzersizliği

E-posta adresi uygulama genelinde benzersiz değildir. Benzersizlik tenant bazlıdır:

```text
(TenantId, Email)
```

Bu nedenle `personel@demo.com` hem `acme` hem de `globex` tenantında seed edilmiştir.

## Outbox Migrationları

`OutboxMessages` tablosu event publish işlemini transaction dışına güvenli şekilde taşır.

Alanlar:

- `EventType`
- `RoutingKey`
- `Payload`
- `CorrelationId`
- `ProcessedAt`
- `DeadLetteredAt`
- `RetryCount`
- `Error`

Mesaj 10 başarısız publish denemesinden sonra `DeadLetteredAt` alanı doldurularak dead-letter durumuna alınır.
