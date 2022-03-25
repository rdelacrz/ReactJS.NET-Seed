const path = require('path');

const currentDir = process.cwd();

module.exports = {
    FRONTEND_DIR: currentDir,
    CONTENT_BASE_DIR: path.resolve(currentDir, 'public'),
    TSCONFIG_PATH: path.resolve(currentDir, 'tsconfig.json'),
    POSTCSS_CONFIG: path.resolve(__dirname, 'postcss.config.js'),
}