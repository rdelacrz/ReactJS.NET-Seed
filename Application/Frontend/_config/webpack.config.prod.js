const path = require('path');
const { ESBuildMinifyPlugin } = require('esbuild-loader');
const { WebpackManifestPlugin } = require('webpack-manifest-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');

const paths = require('./paths');

module.exports = (env, argv) => {
    const outputFolder =  'wwwroot';
    const fileOutputPath = path.resolve(__dirname, `../../${outputFolder}`);

    const appPath = env && env.APP_PATH ? env.APP_PATH : '';

    // Production mode
    return {
        entry: path.resolve(paths.FRONTEND_DIR, 'index.js'),
        module: {
            rules: [
                {
                    test: /\.[jt]sx?$/,
                    exclude: /node_modules\/(?!rxios)/,     // Includes rxios
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: [
                                [
                                    '@babel/preset-env',
                                    { 
                                        modules: false,
                                    },
                                ],
                                '@babel/preset-react',
                                '@babel/typescript',
                            ],
                            plugins: [
                                'babel-plugin-react-scoped-css',    // For react-scoped-css
                                '@babel/plugin-proposal-class-properties',

                                // support dynamic import syntax, but leave it unchanged
                                '@babel/plugin-syntax-dynamic-import',
                            ],
                        }
                    }
                },
            ]
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: '[name].css',
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
                name: 'runtime',
            },
            splitChunks: {
                chunks: 'all',
                cacheGroups: {
                    commons: {
                        test: /[\\/]node_modules[\\/](react|react-dom)[\\/]/,
                        name: 'vendor',
                        chunks: 'all',
                    },
                },
            },
        },
        output: {
            filename: '[name].[contenthash:8].js',
            globalObject: 'this',
            path: fileOutputPath,
            publicPath: `${appPath}/${outputFolder}/`
        }
    };
}