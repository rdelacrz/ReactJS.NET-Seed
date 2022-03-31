import '~~/polyfills';
import React, { FunctionComponent, useEffect, useState } from 'react';
import { dehydrate, Hydrate, QueryClient, QueryClientProvider } from 'react-query';
import { ReactQueryDevtools } from 'react-query/devtools'
import { BrowserRouter } from 'react-router-dom';
import { StaticRouter } from 'react-router-dom/server';
import env from '~~/environment';
import AppRoutes from '~~/routes';

import './app.scss';

interface AppAndRoutingProps {
  location?: string;
}

const Routing: FunctionComponent<AppAndRoutingProps> = (props) => (
  env.isBrowser ? (
    <BrowserRouter basename={env.appPath}>
      <AppRoutes />
    </BrowserRouter>
  ) : (
    <StaticRouter location={props.location}>
      <AppRoutes />
    </StaticRouter>
  )
)

const QueryClientContainer: FunctionComponent<AppAndRoutingProps> = (props) => {
  const [queryClient] = useState(() => new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 10000,
      },
    },
  }));

  const dehydratedState = env.isBrowser
    ? window['__REACT_QUERY_STATE__']
    : dehydrate(queryClient);

  // Clears the React Query cache on the server after render, after dehydrated state has been sent
  useEffect(() => {
    if (env.isServer) {
      queryClient.clear();
    }
  });

  return (
    <QueryClientProvider client={queryClient}>
      <Hydrate state={dehydratedState}>
        <Routing location={props.location} />
      </Hydrate>
      <ReactQueryDevtools />
    </QueryClientProvider>
  );
}

const App: FunctionComponent<AppAndRoutingProps> = (props) => (
  <QueryClientContainer location={props.location} />
);

export default App;
