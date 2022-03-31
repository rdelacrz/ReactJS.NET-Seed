const path = require('path');
const webpack = require('webpack');
const { merge } = require('webpack-merge');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin');

const paths = require('./paths');

const sharedConfig = (env, argv) => {
    const isDev = argv.mode === 'development';
    return {
        mode: argv.mode,
        module: {
            rules: [
                {
                    test: /\.jsx?$/,
                    loader: 'esbuild-loader',
                    options: {
                        loader: 'jsx',
                        target: 'esnext',
                    },
                },
                {
                    test: /\.tsx?$/,
                    loader: 'esbuild-loader',
                    options: {
                        loader: 'tsx',
                        target: 'esnext',
                    },
                },
                {
                    test: /\.css$/,
                    use: [
                        isDev ? 'style-loader' : MiniCssExtractPlugin.loader,
                        {
                            loader: 'css-loader',
                            options: {
                                importLoaders: 1,
                                modules: {
                                    mode: 'icss',
                                },
                            },
                        },
                        { loader: 'scoped-css-loader' },
                    ],
                },
                {
                    test: /\.scss$/,
                    use: [
                        isDev ? 'style-loader' : MiniCssExtractPlugin.loader,
                        {
                            loader: 'css-loader',
                            options: {
                                importLoaders: 1,
                                modules: {
                                    mode: 'icss',
                                },
                            },
                        },
                        { loader: 'scoped-css-loader' },
                        {
                            loader: 'postcss-loader',
                            options: {
                                postcssOptions: {
                                    config: paths.POSTCSS_CONFIG,
                                },
                            },
                        },
                        'sass-loader',
                        {
                            loader: 'sass-resources-loader',
                            options: {
                                resources: require(path.resolve(paths.SRC_DIR, 'styles/references.js')),
                            },
                        },
                    ]
                },
                {
                    test: /\.(pdf|zip|xlsx?)$/,
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]',
                        outputPath: 'documents/'
                    },
                    type: 'javascript/auto',
                },
                {
                    test: /\.(woff(2)?|ttf|otf|eot)(\?v=\d+\.\d+\.\d+)?$/,
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]',
                        outputPath: 'fonts/'
                    },
                    type: 'javascript/auto',
                },
                {
                    test: /\.svg$/,
                    use: [
                        '@svgr/webpack', 
                        {
                            loader: 'file-loader',
                            options: {
                                name: '[name].[ext]',
                                outputPath: 'icons/'
                            },
                        }
                    ],
                    type: 'javascript/auto',
                },
                {
                    test: /\.(png|jpe?g|gif)$/,
                    loader: 'file-loader',
                    options: {
                        name: '[name].[ext]',
                        outputPath: 'images/'
                    },
                    type: 'javascript/auto',
                }
            ]
        },
        resolve: {
            extensions: ['.ts', '.tsx', '.js', '.jsx'],
            mainFields: ['main', 'module'],
            plugins: [
                // Don't need to set aliases manually anymore - uses paths in tsconfig!
                new TsconfigPathsPlugin({
                    configFile: paths.TSCONFIG_PATH,
                }),
            ],
            fallback: {
                'fs': false,
                'path': false,
                'os': false,
            },
        },
        plugins: [
            // Makes variables accessible in application
            new webpack.DefinePlugin({
                'process.env' : {
                    APP_PATH: JSON.stringify(env ? env.APP_PATH : ''),
                    IS_DEV: JSON.stringify(isDev ? 'true' : 'false'),
                },
                SC_DISABLE_SPEEDY: true,
            }),
            new CleanWebpackPlugin(),
        ],
        output: {
            filename: '[name].bundle.js',
        }
    };
}

module.exports = (env, argv) => {
    const config = argv.mode === 'development' ? require('./webpack.config.dev') : require('./webpack.config.prod');
    return merge(sharedConfig(env, argv), config(env, argv));
}