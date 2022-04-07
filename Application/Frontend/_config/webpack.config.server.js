const path = require('path');
const webpack = require('webpack');
const NodePolyfillPlugin = require('node-polyfill-webpack-plugin');

const paths = require('./paths');

module.exports = (env, argv) => {
    const outputFolder =  'wwwroot';
    const fileOutputPath = path.resolve(__dirname, `../../${outputFolder}`);

    const appPath = env && env.APP_PATH ? env.APP_PATH : '';

    // Production mode
    return {
        entry: path.resolve(paths.SRC_DIR, 'index.js'),
        plugins: [
            new webpack.optimize.LimitChunkCountPlugin({
                maxChunks: 1
            }),
            new NodePolyfillPlugin(),
        ],
        output: {
            filename: 'server.js',
            globalObject: 'this',
            path: fileOutputPath,
            publicPath: `${appPath}/${outputFolder}/`
        },
    };
}