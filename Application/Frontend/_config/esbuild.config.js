const autoprefixer = require('autoprefixer');
const { spawn } = require('child_process');
const { build, serve } = require('esbuild');
const { sassPlugin } = require('esbuild-sass-plugin');
const { copyFileSync, existsSync, mkdirSync, readFileSync } = require('fs');
const { createServer, request } = require('http');
const postcss = require('postcss');
const paths = require('./paths');

const globalVarsSCSS = readFileSync(paths.STYLES_GLOBAL_VARS_FILE, 'utf-8');
const mixinsSCSS = readFileSync(paths.STYLES_MIXINS_FILE, 'utf-8');

// Ensures that dist folder exists and moves HTML index file to it
if (!existsSync(paths.DIST_DIR)){
    mkdirSync(paths.DIST_DIR);
}
copyFileSync(paths.DEV_SRC_HTML_FILE, paths.DEV_DIST_HTML_FILE);
console.log(`${paths.DEV_SRC_HTML_FILE} was copied to ${paths.DEV_DIST_HTML_FILE}.`);

// Builds dev application
const port = 3000;
const clients = [];
build({
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

// Serves application in dev server
serve({ servedir: paths.DIST_DIR }, {}).then(() => {
    createServer((req, res) => {
        const { url, method, headers } = req;
        if (req.url === '/esbuild')
            return clients.push(
                res.writeHead(200, {
                    'Content-Type': 'text/event-stream',
                    'Cache-Control': 'no-cache',
                    Connection: 'keep-alive',
                })
            );
        const path = ~url.split('/').pop().indexOf('.') ? url : `/index.html` //for PWA with router
        req.pipe(
            request({ hostname: '0.0.0.0', port: 8000, path, method, headers }, (prxRes) => {
                res.writeHead(prxRes.statusCode, prxRes.headers)
                prxRes.pipe(res, { end: true })
            }),
            { end: true }
        );
    }).listen(port);

    setTimeout(() => {
        const op = { darwin: ['open'], linux: ['xdg-open'], win32: ['cmd', '/c', 'start'] };
        const ptf = process.platform;
        if (clients.length === 0) spawn(op[ptf][0], [...[op[ptf].slice(1)], `http://localhost:${port}`])
    }, 1000); // open the default browser only if it is not opened yet
});