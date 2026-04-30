import { defineBoot } from '#q-app/wrappers';
import { Quasar } from 'quasar';
import tr from 'quasar/lang/tr';

export default defineBoot(() => {
  Quasar.lang.set(tr);
});
