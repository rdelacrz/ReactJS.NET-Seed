import autoprefixer from 'autoprefixer';
import { spawn } from 'child_process';
import esbuild from 'esbuild';
import envPlugin from '@chialab/esbuild-plugin-env';
import { sassPlugin } from 'esbuild-sass-plugin';
import { copyFileSync, existsSync, mkdirSync, readFileSync } from 'fs';
import http from 'http';
import httpProxy from 'http-proxy';
import postcss from 'postcss';
import paths from './paths.js';

const globalVarsSCSS = readFileSync(paths.STYLES_GLOBAL_VARS_FILE, 'utf-8');
const mixinsSCSS = readFileSync(paths.STYLES_MIXINS_FILE, 'utf-8');

// Defines dev server constants that can be adjusted as needed
const DEV_PORT = 3000;
const DEV_SERVER_ENDPOINT = 'http://localhost:50486';

// Ensures that dist folder exists and moves HTML index file to it
if (!existsSync(paths.DIST_DIR)) {
    mkdirSync(paths.DIST_DIR);
}
copyFileSync(paths.DEV_SRC_HTML_FILE, paths.DEV_DIST_HTML_FILE);


///////////////////////////////////////////////////////////////// 
//  Builds initial application for the dev environment
/////////////////////////////////////////////////////////////////
const clients = [];
esbuild.build({
    entryPoints: [paths.DEV_ENTRY_FILE],
    bundle: true,
    outdir: '/dist',
    banner: { js: ' (() => new EventSource("/esbuild").onmessage = () => location.reload())();' },
    watch: {
        onRebuild(error, result) {
            clients.forEach((res) => res.write('data: update\n\n'))
            clients.length = 0
            console.log(error ? error : '...')
        },
    },
    plugins: [
        envPlugin(),
        sassPlugin({
            precompile(source, pathname) {
                const precompileSCSS = globalVarsSCSS + '\n' + mixinsSCSS;
                const prefix = /_.+\.scss$/.test(pathname) ? '' : precompileSCSS;
                return prefix + source;
            },
            async transform(source) {
                const { css } = await postcss([autoprefixer]).process(
                    source,
                    { from: false }
                );
                return css;
            },
        }),
    ],
}).catch(() => process.exit(1));

///////////////////////////////////////////////////////////////// 
//  Serves newly-build app on esbuild dev server via proxy
/////////////////////////////////////////////////////////////////
esbuild.serve({ servedir: paths.DIST_DIR }, {}).then(({ host, port }) => {
    console.log(`Serving app on: \u001b[36mhttp://localhost:${DEV_PORT}\u001b[0m`);

    const apiProxy = httpProxy.createProxyServer();
    http.createServer((req, res) => {
        const { url, method, headers } = req;

        if (url === '/esbuild') {
            return clients.push(
                res.writeHead(200, {
                    'Content-Type': 'text/event-stream',
                    'Cache-Control': 'no-cache',
                    Connection: 'keep-alive',
                })
            );
        } else if (url.startsWith('/api')) {
            // Handles API calls and redirects them to API backend server rather than to esbuild's dev server
            return apiProxy.web(req, res, {
                target: DEV_SERVER_ENDPOINT,
                secure: false,
                changeOrigin: true,
            }, (err, proxyRes) => {
                if (proxyRes.statusCode) {
                    res.writeHead(proxyRes.statusCode, proxyRes.headers);
                    proxyRes.pipe(res, { end: true });
                }
            });
        }

        // Forward each incoming request to esbuild
        const path = ~url.split('/').pop().indexOf('.') ? url : `/index.html`;  // For PWA with router
        const proxyReq = http.request({ hostname: host, port, path, method, headers }, (proxyRes) => {
            // Forward the response from esbuild to the client
            res.writeHead(proxyRes.statusCode, proxyRes.headers);
            proxyRes.pipe(res, { end: true });
        });

        // Forward the body of the request to esbuild
        req.pipe(proxyReq, { end: true });
    }).listen(DEV_PORT);

    // Open the default browser only if it is not opened yet
    setTimeout(() => {
        const op = { darwin: ['open'], linux: ['xdg-open'], win32: ['cmd', '/c', 'start'] };
        const ptf = process.platform;
        if (clients.length === 0) spawn(op[ptf][0], [...[op[ptf].slice(1)], `http://localhost:${DEV_PORT}`]);
    }, 1000);
});
