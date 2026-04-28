#!/bin/bash
# Keycloak realm import sonrasında service-account-expense-service kullanıcısına
# realm-management rollerini atar.
# Bu script Keycloak başladıktan sonra bir kez çalıştırılmalıdır.

set -e

KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:18080}"
REALM="izometri"
ADMIN_USER="${KEYCLOAK_ADMIN:-admin}"
ADMIN_PASS="${KEYCLOAK_ADMIN_PASSWORD:-admin}"
CLIENT_ID="expense-service"

echo "⏳ Waiting for Keycloak to be ready..."
until curl -sf "${KEYCLOAK_URL}/health/ready" > /dev/null 2>&1; do
  sleep 2
done
echo "✅ Keycloak is ready"

# 1. Admin token al
echo "🔑 Getting admin token..."
ADMIN_TOKEN=$(curl -sf -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=admin-cli" \
  -d "username=${ADMIN_USER}" \
  -d "password=${ADMIN_PASS}" \
  -d "grant_type=password" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])" 2>/dev/null) || {
  # python yoksa jq dene
  ADMIN_TOKEN=$(curl -sf -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "grant_type=password" \
    -d "client_id=admin-cli" \
    -d "username=${ADMIN_USER}" \
    -d "password=${ADMIN_PASS}" | jq -r '.access_token')
}

if [ -z "$ADMIN_TOKEN" ] || [ "$ADMIN_TOKEN" = "null" ]; then
  echo "❌ Failed to get admin token"
  exit 1
fi
echo "✅ Admin token acquired"

# 2. expense-service client'ın ID'sini bul
echo "🔍 Finding expense-service client..."
CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=${CLIENT_ID}" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])" 2>/dev/null) || {
  CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=${CLIENT_ID}" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq -r '.[0].id')
}
echo "   Client UUID: ${CLIENT_UUID}"

# 3. Service account user'ın ID'sini bul
echo "🔍 Finding service account user..."
SA_USER_ID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${CLIENT_UUID}/service-account-user" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])" 2>/dev/null) || {
  SA_USER_ID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${CLIENT_UUID}/service-account-user" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq -r '.id')
}
echo "   Service Account User ID: ${SA_USER_ID}"

# 4. realm-management client'ın ID'sini bul
echo "🔍 Finding realm-management client..."
RM_CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=realm-management" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" | python3 -c "import sys,json; print(json.load(sys.stdin)[0]['id'])" 2>/dev/null) || {
  RM_CLIENT_UUID=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=realm-management" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}" | jq -r '.[0].id')
}
echo "   Realm-Management Client UUID: ${RM_CLIENT_UUID}"

# 5. Gerekli rollerin ID'lerini bul ve ata
ROLES_TO_ASSIGN=("manage-users" "view-users" "manage-realm" "view-realm")
ROLE_PAYLOAD="["

for ROLE_NAME in "${ROLES_TO_ASSIGN[@]}"; do
  echo "🔍 Finding role: ${ROLE_NAME}..."
  ROLE_JSON=$(curl -sf "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${RM_CLIENT_UUID}/roles/${ROLE_NAME}" \
    -H "Authorization: Bearer ${ADMIN_TOKEN}")
  
  ROLE_ID=$(echo "$ROLE_JSON" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])" 2>/dev/null) || {
    ROLE_ID=$(echo "$ROLE_JSON" | jq -r '.id')
  }
  
  if [ -n "$ROLE_PAYLOAD" ] && [ "$ROLE_PAYLOAD" != "[" ]; then
    ROLE_PAYLOAD="${ROLE_PAYLOAD},"
  fi
  ROLE_PAYLOAD="${ROLE_PAYLOAD}{\"id\":\"${ROLE_ID}\",\"name\":\"${ROLE_NAME}\"}"
  echo "   ${ROLE_NAME}: ${ROLE_ID}"
done

ROLE_PAYLOAD="${ROLE_PAYLOAD}]"

# 6. Rolleri service account user'a ata
echo "📋 Assigning roles to service account..."
HTTP_CODE=$(curl -sf -o /dev/null -w "%{http_code}" -X POST \
  "${KEYCLOAK_URL}/admin/realms/${REALM}/users/${SA_USER_ID}/role-mappings/clients/${RM_CLIENT_UUID}" \
  -H "Authorization: Bearer ${ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d "${ROLE_PAYLOAD}")

if [ "$HTTP_CODE" = "204" ] || [ "$HTTP_CODE" = "200" ]; then
  echo "✅ Roles assigned successfully!"
else
  echo "⚠️  Role assignment returned HTTP ${HTTP_CODE} (may already be assigned)"
fi

echo ""
echo "🎉 Done! expense-service service account now has manage-users and view-users roles."
