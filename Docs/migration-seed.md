# Migration ve Seed Bilgileri

## Migration Komutlari

Bu repoda migration gecmisi teslim oncesi sade tutulur: ExpenseService ve NotificationService icin birer `InitialCreate` migration vardir.

Expense DB migration olusturma:

```bash
dotnet ef migrations add InitialCreate --context ExpenseDbContext --project src/Services/ExpenseService/ExpenseService.Infrastructure/ExpenseService.Infrastructure.csproj --startup-project src/Services/ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj --output-dir Persistence/Migrations
```

Notification DB migration olusturma:

```bash
dotnet ef migrations add InitialCreate --context NotificationDbContext --project src/Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj --startup-project src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj --output-dir Persistence/Migrations
```

Expense DB migration uygulama:

```bash
dotnet ef database update --project src/Services/ExpenseService/ExpenseService.Infrastructure/ExpenseService.Infrastructure.csproj --startup-project src/Services/ExpenseService/ExpenseService.Api/ExpenseService.Api.csproj
```

Notification DB migration uygulama:

```bash
dotnet ef database update --project src/Services/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj --startup-project src/Services/NotificationService/NotificationService.Api/NotificationService.Api.csproj
```

Docker Compose ile calistirildiginda API containerlari migrationlari startup sirasinda otomatik uygular.

## Expense DB Tablolari

- `Tenants`
- `Users`
- `UserRoles`
- `Expenses`
- `ExpenseApprovals`
- `OutboxMessages`
- `__EFMigrationsHistory`

## Notification DB Tablolari

- `Notifications`
- `ProcessedMessages`
- `NotificationDeadLetters`
- `__EFMigrationsHistory`

## Seed Tenantlar

| TenantId | Tenant |
| --- | --- |
| `10000000-0000-0000-0000-000000000001` | `izometri` |
| `10000000-0000-0000-0000-000000000002` | `test1` |
| `10000000-0000-0000-0000-000000000003` | `test2` |

## Seed Kullanicilar

Tum kullanicilar icin sifre: `Pass123!`

| Tenant | UserId | E-posta | Roller |
| --- | --- | --- | --- |
| `izometri` | `20000000-0000-0000-0000-000000000001` | `admin@izometri.com` | Admin |
| `izometri` | `20000000-0000-0000-0000-000000000002` | `hr@izometri.com` | HR |
| `izometri` | `20000000-0000-0000-0000-000000000003` | `personel@izometri.com` | Personel |
| `izometri` | `20000000-0000-0000-0000-000000000010` | `personel2@izometri.com` | Personel |
| `test1` | `20000000-0000-0000-0000-000000000004` | `pattabanoglu@devrimmehmet.com` | Admin |
| `test1` | `20000000-0000-0000-0000-000000000005` | `devrimmehmet@gmail.com` | HR |
| `test1` | `20000000-0000-0000-0000-000000000006` | `devrimmehmet@msn.com` | Personel |
| `test1` | `20000000-0000-0000-0000-000000000011` | `personel2@test1.com` | Personel |
| `test2` | `20000000-0000-0000-0000-000000000007` | `admin@test2.com` | Admin |
| `test2` | `20000000-0000-0000-0000-000000000008` | `hr@test2.com` | HR |
| `test2` | `20000000-0000-0000-0000-000000000009` | `personel@test2.com` | Personel |
| `test2` | `20000000-0000-0000-0000-000000000012` | `personel2@test2.com` | Personel |

Bu liste `deploy/keycloak/izometri-realm.json` icindeki Keycloak kullanicilariyla ayni `UserId`, `TenantId`, email ve rol degerlerini kullanir.

## E-posta Benzersizligi

E-posta adresi uygulama genelinde benzersiz degildir. Benzersizlik tenant bazlidir:

```text
(TenantId, Email)
```

## Seed Harcamalar

`test1` tenantinda farkli status, kategori ve approval threshold senaryolarini gosteren ornek harcamalar seed edilir.

## Outbox Migrationlari

`OutboxMessages` tablosu event publish islemini transaction disina guvenli sekilde tasir.

Alanlar:

- `EventType`
- `RoutingKey`
- `Payload`
- `CorrelationId`
- `ProcessedAt`
- `DeadLetteredAt`
- `RetryCount`
- `Error`

Mesaj 10 basarisiz publish denemesinden sonra `DeadLetteredAt` alani doldurularak dead-letter durumuna alinir.
