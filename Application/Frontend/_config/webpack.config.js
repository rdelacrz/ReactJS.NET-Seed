const path = require('path');
const webpack = require('webpack');
const { merge } = require('webpack-merge');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin');

const paths = require('./paths');

const sharedConfig = (env, argv, ssr) => {
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
                    type: 'asset/resource',
                    generator : {
                      filename : 'documents/[name][ext]',
                    },
                },
                {
                    test: /\.(woff(2)?|ttf|otf|eot)(\?v=\d+\.\d+\.\d+)?$/,
                    type: 'asset/resource',
                    generator : {
                      filename : 'fonts/[name][ext]',
                    },
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
                    type: 'asset/resource',
                    generator : {
                      filename : 'images/[name][ext]',
                    },
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
                'buffer': require.resolve('buffer'),
                'fs': false,
                'path': false,
                'os': false,
                'http': require.resolve('stream-http'),
                'https': require.resolve('https-browserify'),
                'stream': require.resolve('stream-browserify'),
                'zlib': require.resolve('browserify-zlib')
            },
        },
        plugins: [
            // Makes variables accessible in application
            new webpack.DefinePlugin({
                'process.env' : {
                    APP_PATH: JSON.stringify(env ? env.APP_PATH : ''),
                    IS_DEV: JSON.stringify(isDev ? 'true' : 'false'),
                    SSR: JSON.stringify(ssr ? 'true' : 'false'),
                },
                SC_DISABLE_SPEEDY: true,    // For styled components in production
            }),
            new MiniCssExtractPlugin(),
        ],
        output: {
            filename: '[name].bundle.js',
        }
    };
}

module.exports = (env, argv) => {
    const clientConfig = require('./webpack.config.client');
    const serverConfig = require('./webpack.config.server');
    return [
        merge(sharedConfig(env, argv, false), clientConfig(env, argv)),
        merge(sharedConfig(env, argv, true), serverConfig(env, argv)),
    ];
}