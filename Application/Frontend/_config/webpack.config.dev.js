const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ReactRefreshWebpackPlugin = require('@pmmmwh/react-refresh-webpack-plugin');

const paths = require('./paths');

module.exports = (env, argv) => {
    return {
        entry: path.resolve(paths.FRONTEND_DIR, 'index.dev.jsx'),
        cache: {
            // 1. Set cache type to filesystem
            type: 'filesystem',
        
            buildDependencies: {
                // 2. Add your config as buildDependency to get cache invalidation on config change
                config: [__filename],
            
                // 3. If you have other things the build depends on you can add them here
                // Note that webpack, loaders and all modules referenced from your config are automatically added
            },
        },
        watchOptions: {
            ignored: /node_modules/
        },
        devtool: 'inline-source-map',
        devServer: {
            static: paths.CONTENT_BASE_DIR,
            historyApiFallback: true,
            proxy: {
                '/api/**':  {
                    target: 'https://localhost:44388',
                    secure: false
                }
            },
        },
        plugins: [
            new HtmlWebpackPlugin({
                title: 'Development Server',
                template: path.resolve(paths.FRONTEND_DIR, 'index.html'),
                favicon: path.resolve(paths.CONTENT_BASE_DIR, 'favicon.ico'),
            }),
            new ReactRefreshWebpackPlugin(),
        ],
        output: {
            publicPath: '/'
        }
    }
}