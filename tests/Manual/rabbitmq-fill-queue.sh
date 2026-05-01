#!/usr/bin/env bash
set -euo pipefail

COUNT="${1:-250}"
QUEUE="${RABBITMQ_DEBUG_QUEUE:-debug.expense-events}"
EXCHANGE="${RABBITMQ_EXCHANGE:-expense.events}"
ROUTING_KEY="${RABBITMQ_ROUTING_KEY:-debug.expense.created}"
RABBITMQ_URL="${RABBITMQ_MANAGEMENT_URL:-http://localhost:15673}"
RABBITMQ_USER="${RABBITMQ_USER:-izometri}"
RABBITMQ_PASS="${RABBITMQ_PASS:-Izometri2026!}"
TENANT_ID="${RABBITMQ_TEST_TENANT_ID:-10000000-0000-0000-0000-000000000001}"

usage() {
  cat <<EOF
Usage:
  tests/Manual/rabbitmq-fill-queue.sh [count]

Environment:
  RABBITMQ_DEBUG_QUEUE      Queue to fill. Default: debug.expense-events
  RABBITMQ_EXCHANGE         Exchange. Default: expense.events
  RABBITMQ_ROUTING_KEY      Routing key. Default: debug.expense.created
  RABBITMQ_MANAGEMENT_URL   Management URL. Default: http://localhost:15673
  RABBITMQ_USER             Management user. Default: izometri
  RABBITMQ_PASS             Management password. Default: Izometri2026!

Examples:
  tests/Manual/rabbitmq-fill-queue.sh 500
  RABBITMQ_ROUTING_KEY=expense.created tests/Manual/rabbitmq-fill-queue.sh 100

Open RabbitMQ Management:
  ${RABBITMQ_URL}

Then inspect:
  Queues and Streams -> ${QUEUE}
EOF
}

if [[ "${COUNT}" == "-h" || "${COUNT}" == "--help" ]]; then
  usage
  exit 0
fi

if ! [[ "${COUNT}" =~ ^[0-9]+$ ]] || [[ "${COUNT}" -lt 1 ]]; then
  echo "count must be a positive integer." >&2
  exit 1
fi

for tool in curl jq; do
  if ! command -v "${tool}" >/dev/null 2>&1; then
    echo "Missing required tool: ${tool}" >&2
    exit 1
  fi
done

api() {
  curl -fsS -u "${RABBITMQ_USER}:${RABBITMQ_PASS}" "$@"
}

vhost="%2F"

if [[ "${QUEUE}" == debug.* ]]; then
  api -X DELETE "${RABBITMQ_URL}/api/queues/${vhost}/${QUEUE}" >/dev/null 2>&1 || true
fi

echo "Declaring durable debug queue '${QUEUE}' and binding '${EXCHANGE}' -> '${ROUTING_KEY}'..."
api -H "content-type: application/json" \
  -X PUT "${RABBITMQ_URL}/api/queues/${vhost}/${QUEUE}" \
  -d '{"durable":true,"auto_delete":false,"arguments":{}}' >/dev/null

api -H "content-type: application/json" \
  -X POST "${RABBITMQ_URL}/api/bindings/${vhost}/e/${EXCHANGE}/q/${QUEUE}" \
  -d "$(jq -n --arg routing_key "${ROUTING_KEY}" '{routing_key:$routing_key, arguments:{}}')" >/dev/null

echo "Publishing ${COUNT} test messages to exchange '${EXCHANGE}' with routing key '${ROUTING_KEY}'..."
for i in $(seq 1 "${COUNT}"); do
  event_id="$(cat /proc/sys/kernel/random/uuid)"
  expense_id="$(cat /proc/sys/kernel/random/uuid)"
  occurred_at="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  payload="$(jq -nc \
    --arg eventId "${event_id}" \
    --arg correlationId "rabbitmq-panel-fill-${i}" \
    --arg occurredAt "${occurred_at}" \
    --arg tenantId "${TENANT_ID}" \
    --arg expenseId "${expense_id}" \
    --arg requestedBy "00000000-0000-0000-0000-000000000001" \
    --arg currency "TRY" \
    '{
      EventId: $eventId,
      CorrelationId: $correlationId,
      OccurredAt: $occurredAt,
      TenantId: $tenantId,
      ExpenseId: $expenseId,
      RequestedBy: $requestedBy,
      Amount: 1,
      Currency: $currency,
      RecipientEmail: "",
      RecipientPhone: ""
    }')"

  response="$(api -H "content-type: application/json" \
    -X POST "${RABBITMQ_URL}/api/exchanges/${vhost}/${EXCHANGE}/publish" \
    -d "$(jq -nc \
      --arg routing_key "${ROUTING_KEY}" \
      --arg payload "${payload}" \
      --arg correlation_id "rabbitmq-panel-fill-${i}" \
      '{
        properties: {
          content_type: "application/json",
          delivery_mode: 2,
          correlation_id: $correlation_id
        },
        routing_key: $routing_key,
        payload: $payload,
        payload_encoding: "string"
      }')")"

  if [[ "$(jq -r '.routed' <<<"${response}")" != "true" ]]; then
    echo "Message ${i} was not routed. Response: ${response}" >&2
    exit 1
  fi

  if (( i % 50 == 0 || i == COUNT )); then
    echo "Published ${i}/${COUNT}"
  fi
done

echo
echo "Queue snapshot:"
api "${RABBITMQ_URL}/api/queues/${vhost}/${QUEUE}" | jq '{
  name,
  state,
  consumers,
  messages,
  messages_ready,
  messages_unacknowledged,
  publish: .message_stats.publish,
  deliver: .message_stats.deliver,
  ack: .message_stats.ack
}'

echo
echo "Open ${RABBITMQ_URL} -> Queues and Streams -> ${QUEUE}"
echo "To empty the debug queue later:"
echo "  curl -u '${RABBITMQ_USER}:${RABBITMQ_PASS}' -X DELETE '${RABBITMQ_URL}/api/queues/%2F/${QUEUE}/contents'"
