# Keycloak Import Rehberi

Bu projede production/Docker kimlik dogrulama kaynagi Keycloak'tur. API'ler kullanici tokeni uretmez; sadece JWT Bearer access token dogrular.

- `UserId`: Expense DB seed kullanici GUID'i
- `TenantId`: Expense DB seed tenant GUID'i
- `role`: `Admin`, `HR`, `Personel` veya `Service`
- `aud`: `expense-service`

Authorization API tarafinda JWT claimleri uzerinden yapilir. Tenant izolasyonu `TenantId` claim'iyle, rol kontrolleri `role` claim'iyle calisir.

## Baslatma

Keycloak ana Docker Compose akışına dahildir:

```bash
docker compose up -d --build
```

Keycloak yonetim paneli:

- URL: `http://localhost:18080`
- Kullanici adi: `admin`
- Sifre: `admin`

Compose calistiginda `deploy/keycloak/izometri-realm.json` dosyasi container icine read-only baglanir ve Keycloak `start-dev --import-realm` komutuyla baslar.

## Import Edilen Realm

- Realm: `izometri`
- Client: `expense-service`
- Client secret: `expense-service-client-secret`
- Roller: `Admin`, `HR`, `Personel`, `Service`
- Protocol mapper'lar:
  - Realm rollerini `role` claim'i olarak yazar.
  - Kullanici attribute'larindan `UserId` ve `TenantId` claim'lerini yazar.
  - Access token'a `expense-service` audience degerini ekler.

## Seed Kullanici Eslesmesi

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

## Token Alma

Local makineden seeded demo kullanicisi icin Keycloak access token almak:

```bash
curl -X POST "http://localhost:18080/realms/izometri/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=expense-service" \
  -d "client_secret=expense-service-client-secret" \
  -d "grant_type=password" \
  -d "username=devrimmehmet@msn.com" \
  -d "password=Pass123!"
```

## API Ayari

Container icinden Keycloak kullanimi:

```bash
Jwt__Authority=http://keycloak:8080/realms/izometri
Jwt__PublicAuthority=http://localhost:18080/realms/izometri
Jwt__Audience=expense-service
Jwt__RequireHttpsMetadata=false
Authentication__EnableLocalLogin=false
```

Local host uzerinden `dotnet run` ile Keycloak kullanimi:

```bash
Jwt__Authority=http://localhost:18080/realms/izometri
Jwt__Audience=expense-service
Jwt__RequireHttpsMetadata=false
```

Bu ayarlar hem ExpenseService hem NotificationService icin gecerlidir. `Jwt:Authority` API containerlarinin Keycloak metadata'sina erismesi icin internal Docker adresidir. `Jwt:PublicAuthority` ise hosttan veya browserdan alinan tokenlardaki issuer degerini kabul etmek icindir.

`/api/auth/login` endpointi local gelistirme ve test icin opsiyonel bir fallback olarak kodda durur. Docker/prod akista `Authentication:EnableLocalLogin=false` oldugu icin kullanici tokeni API tarafindan uretilmez.
