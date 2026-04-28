#!/bin/sh
# Keycloak post-start setup — Docker init container tarafından çalıştırılır.
# Gereksinimler: curl, jq (Alpine'de: apk add curl jq)
# Yapılanlar:
#   1. User Profile'da unmanagedAttributePolicy=ADMIN_EDIT (userId/tenantId gibi custom attr izni)
#   2. expense-service için roles/web-origins/acr/basic default scope eklenir
#   3. Servis hesabına manage-users, view-users, manage-realm, view-realm rolleri atanır
# İdempotent: birden fazla çalıştırılması güvenlidir.

set -e

KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:18080}"
# Management port (9000 inside Docker, 19000 on host) — health endpoint buradadır
KEYCLOAK_MGMT_URL="${KEYCLOAK_MGMT_URL:-${KEYCLOAK_URL%:*}:9000}"
REALM="izometri"
ADMIN_USER="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PASS="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
CLIENT_ID="expense-service"

# ── Keycloak hazır olana kadar bekle ──────────────────────────────────────────
echo "[setup] Waiting for Keycloak at ${KEYCLOAK_MGMT_URL}/health/ready ..."
until curl -sf "${KEYCLOAK_MGMT_URL}/health/ready" > /dev/null 2>&1; do
  sleep 3
done
echo "[setup] Keycloak is ready."

# ── Admin token ───────────────────────────────────────────────────────────────
ADMIN_TOKEN=$(curl -sf -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  --data-urlencode "grant_type=password" \
  --data-urlencode "client_id=admin-cli" \
  --data-urlencode "username=${ADMIN_USER}" \
  --data-urlencode "password=${ADMIN_PASS}" \
  | jq -r '.access_token')

if [ -z "$ADMIN_TOKEN" ] || [ "$ADMIN_TOKEN" = "null" ]; then
  echo "[setup] ERROR: Failed to get admin token"
  exit 1
fi
echo "[setup] Admin token acquired."

# ── User Profile: unmanaged attribute'lara izin ver (userId, tenantId vb.) ───
# Keycloak 25'te Declarative User Profile varsayılan aktif; tanımlanmamış attr'lar
# sessizce siliniyor. ADMIN_EDIT → admin API'nin custom attribute set etmesine izin verir.
PROFILE=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/users/profile" \
  -H "Authorization: Bearer $ADMIN_TOKEN")
UPDATED_PROFILE=$(echo "$PROFILE" | jq '.unmanagedAttributePolicy = "ADMIN_EDIT"')
HTTP=$(curl -sf -o /dev/null -w "%{http_code}" -X PUT \
  "${KEYCLOAK_URL}/admin/realms/${REALM}/users/profile" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "$UPDATED_PROFILE")
echo "[setup] User profile unmanagedAttributePolicy=ADMIN_EDIT: HTTP ${HTTP}"

# ── expense-service client UUID ───────────────────────────────────────────────
CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=${CLIENT_ID}" \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r '.[0].id')
echo "[setup] expense-service UUID: ${CLIENT_UUID}"

# ── Default client scope'ları ekle ───────────────────────────────────────────
ALL_SCOPES=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/client-scopes" \
  -H "Authorization: Bearer $ADMIN_TOKEN")

for SCOPE_NAME in roles web-origins acr basic; do
  SCOPE_ID=$(echo "$ALL_SCOPES" | jq -r --arg name "$SCOPE_NAME" '.[] | select(.name==$name) | .id')
  if [ -n "$SCOPE_ID" ] && [ "$SCOPE_ID" != "null" ]; then
    HTTP=$(curl -sf -o /dev/null -w "%{http_code}" -X PUT \
      "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${CLIENT_UUID}/default-client-scopes/${SCOPE_ID}" \
      -H "Authorization: Bearer $ADMIN_TOKEN")
    echo "[setup] Scope '${SCOPE_NAME}': HTTP ${HTTP}"
  else
    echo "[setup] WARN: scope '${SCOPE_NAME}' not found, skipping."
  fi
done

# ── Servis hesabı ID'sini bul ─────────────────────────────────────────────────
SA_USER_ID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${CLIENT_UUID}/service-account-user" \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r '.id')
echo "[setup] Service account user ID: ${SA_USER_ID}"

# ── realm-management client UUID ─────────────────────────────────────────────
RM_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=realm-management" \
  -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r '.[0].id')
echo "[setup] realm-management UUID: ${RM_UUID}"

# ── Gerekli rolleri ata ───────────────────────────────────────────────────────
ROLE_PAYLOAD=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${RM_UUID}/roles" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  | jq '[.[] | select(.name=="manage-users" or .name=="view-users" or .name=="manage-realm" or .name=="view-realm") | {id,name}]')

HTTP=$(curl -sf -o /dev/null -w "%{http_code}" -X POST \
  "${KEYCLOAK_URL}/admin/realms/${REALM}/users/${SA_USER_ID}/role-mappings/clients/${RM_UUID}" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d "$ROLE_PAYLOAD")

if [ "$HTTP" = "204" ] || [ "$HTTP" = "200" ]; then
  echo "[setup] Service account roles assigned."
else
  echo "[setup] WARN: Role assignment returned HTTP ${HTTP} (may already be assigned)."
fi

echo "[setup] Done — expense-service is ready to sync users to Keycloak."
