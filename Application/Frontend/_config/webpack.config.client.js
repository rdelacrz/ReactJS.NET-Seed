const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const { ESBuildMinifyPlugin } = require('esbuild-loader');
const webpack = require('webpack');
const { WebpackManifestPlugin }  = require('webpack-manifest-plugin');

const paths = require('./paths');

module.exports = (env, argv) => {
    const outputFolder =  'wwwroot';
    const fileOutputPath = path.resolve(__dirname, `../../${outputFolder}`);

    const appPath = env && env.APP_PATH ? env.APP_PATH : '';

    // Production mode
    return {
        target: 'web',
        entry: path.resolve(paths.SRC_DIR, 'index.js'),
        optimization: {
            moduleIds: 'deterministic',
            minimizer: [
                '...',
                new ESBuildMinifyPlugin({
                    target: 'esnext',
                    css: true,
                }),
            ],
            runtimeChunk: {
                name: 'runtime', // necessary when using multiple entrypoints on the same page
            },
            splitChunks: {
                cacheGroups: {
                    commons: {
                        test: /[\\/]node_modules[\\/](react|react-dom)[\\/]/,
                        name: 'vendor',
                        chunks: 'all',
                    },
                },
            },
        },
        plugins: [
            new CleanWebpackPlugin(),
            new CopyPlugin({
                patterns: [
                    { 
                        from: 'src/public',
                    },
                ],
            }),
            new webpack.ProvidePlugin({
                Buffer: ['buffer', 'Buffer'],
                process: 'process/browser',
            }),
            new WebpackManifestPlugin({
                fileName: 'asset-manifest.json',
                generate: (seed, files) => {
                    const manifestFiles = files.reduce((manifest, file) => {
                        manifest[file.name] = file.path;
                        return manifest;
                    }, seed);
    
                    const entrypointFiles = files.filter(x => x.isInitial && !x.name.endsWith('.map')).map(x => x.path);
    
                    return {
                        files: manifestFiles,
                        entrypoints: entrypointFiles,
                    };
                },
            }),
        ],
        output: {
            filename: 'client-[name].[contenthash:8].js',
            globalObject: 'this',
            path: fileOutputPath,
            publicPath: `${appPath}/${outputFolder}/`
        }
    };
}