/**
 * Polyfills that should only be run browser-side (because they break when run on the server-side).
 */

// Required for foreach calls
import 'nodelist-foreach-polyfill';

// Required for classlists
import 'classlist-polyfill';