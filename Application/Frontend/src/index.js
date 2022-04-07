import React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';
import { dehydrate, Hydrate, QueryClient, QueryClientProvider } from 'react-query';
import { App, queryClientOptions } from './components/app';
import env from './environment';
import { setTimeout, clearTimeout } from './timeout-shim';

global.React = React;
global.ReactDOM = ReactDOM;

if (env.isServer) {
    global.ReactDOMServer = ReactDOMServer;
    global.queryClientOptions = queryClientOptions;
    global.dehydrate = dehydrate;
    global.Hydrate = Hydrate;
    global.QueryClient = QueryClient;
    global.QueryClientProvider = QueryClientProvider;
    global.setTimeout = setTimeout;
    global.clearTimeout = clearTimeout;
}

global.Components = { App };
