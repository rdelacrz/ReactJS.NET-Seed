// IE9, IE10 and IE11 requires all of the following polyfills
import 'core-js/features/array';
import 'core-js/features/date';
import 'core-js/features/function';
import 'core-js/features/map';
import 'core-js/features/math';
import 'core-js/features/number';
import 'core-js/features/object';
import 'core-js/features/parse-float';
import 'core-js/features/parse-int';
import 'core-js/features/set';
import 'core-js/features/string';
import 'core-js/features/symbol';
import 'core-js/features/regexp';
import 'core-js/features/url';
import 'core-js/features/url-search-params';

// Replaces @babel/polyfill
import 'core-js/stable';
import 'regenerator-runtime/runtime';

// Evergreen browsers require these
import 'core-js/features/reflect';
import 'reflect-metadata';

// Required for promises
import 'promise-polyfill/src/polyfill';

if (typeof window !== 'undefined') {
  require('./browser.polyfill');
}
