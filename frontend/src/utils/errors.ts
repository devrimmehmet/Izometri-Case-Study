import { copyToClipboard, type QNotifyCreateOptions, type QVueGlobals } from 'quasar';
import { parseApiError } from 'src/services/http';

export function notifyApiError($q: QVueGlobals, error: unknown, fallback?: string) {
  const details = parseApiError(error, fallback);

  const options: QNotifyCreateOptions = {
    type: 'negative',
    message: details.message,
  };

  if (details.correlationId) {
    options.caption = `Correlation ID: ${details.correlationId}`;
    options.actions = [
      {
        label: 'Kopyala',
        color: 'white',
        handler: () => {
          void copyToClipboard(details.correlationId ?? '');
        },
      },
    ];
  }

  $q.notify(options);
}
