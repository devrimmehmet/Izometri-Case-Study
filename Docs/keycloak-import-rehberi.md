# Keycloak Import Rehberi

Bu proje basit JWT login akışını varsayılan olarak korur. Keycloak desteği ise ek puan kapsamındaki OAuth2 hazırlığı için opsiyonel `oauth` profiliyle çalışır.

## Başlatma

```bash
docker compose --profile oauth up -d keycloak
```

Keycloak yönetim paneli:

- URL: `http://localhost:18080`
- Kullanıcı adı: `admin`
- Şifre: `admin`

## Otomatik Import

Compose profili açıldığında `deploy/keycloak/izometri-realm.json` dosyası container içine read-only bağlanır ve Keycloak `start-dev --import-realm` komutuyla başlar.

Import edilen ana parçalar:

- Realm: `izometri`
- Client: `expense-service`
- Client secret: `expense-service-client-secret`
- Roller: `Admin`, `HR`, `Personnel`, `Service`
- Demo kullanıcılar:
  - `admin@acme.com`
  - `hr@acme.com`
  - `personnel@acme.com`

Tüm demo kullanıcıların şifresi:

```text
Passw0rd!
```

## API Ayarı

ExpenseService Keycloak doğrulamasına alınacaksa JWT ayarlarına authority değeri verilir:

```yaml
Jwt__Authority: http://keycloak:8080/realms/izometri
Jwt__RequireHttpsMetadata: false
Jwt__Issuer: http://keycloak:8080/realms/izometri
Jwt__Audience: expense-service
```

Mevcut case akışında tenant izolasyonu için `TenantId` claim’i gerekir. Keycloak kullanımı genişletilecekse client mapper ile `TenantId` ve rol claim formatı API’nin beklediği hale getirilmelidir.
