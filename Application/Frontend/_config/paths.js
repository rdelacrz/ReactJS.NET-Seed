const path = require('path');

const currentDir = process.cwd();

module.exports = {
    FRONTEND_DIR: currentDir,
    SRC_DIR: path.resolve(currentDir, 'src'),
    DIST_DIR: path.resolve(currentDir, 'dist'),
    DEV_ENTRY_FILE: path.resolve(currentDir, 'src', 'index.dev.jsx'),
    DEV_SRC_HTML_FILE: path.resolve(currentDir, 'src', 'index.html'),
    DEV_DIST_HTML_FILE: path.resolve(currentDir, 'dist', 'index.html'),
    STYLES_GLOBAL_VARS_FILE: path.resolve(currentDir, 'src', 'styles', '_global_variables.scss'),
    STYLES_MIXINS_FILE: path.resolve(currentDir, 'src', 'styles', '_mixins.scss'),
    CONTENT_BASE_DIR: path.resolve(currentDir, 'src', 'public'),
    TSCONFIG_PATH: path.resolve(currentDir, 'tsconfig.json'),
    POSTCSS_CONFIG: path.resolve(__dirname, 'postcss.config.js'),
}