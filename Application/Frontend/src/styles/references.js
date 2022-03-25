/* All global variable and function references should be imported here */

const path = require('path');

const resources = [
  '_global_variables.scss',
  '_mixins.scss',
];

module.exports = resources.map(file => path.resolve(__dirname, file));
