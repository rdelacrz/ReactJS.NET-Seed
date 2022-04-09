import '~/polyfills';
import React, { FunctionComponent, useState } from 'react';
import { Hydrate, QueryClient, QueryClientConfig, QueryClientProvider } from 'react-query';
import { ReactQueryDevtools } from 'react-query/devtools'
import { BrowserRouter } from 'react-router-dom';
import { StaticRouter } from 'react-router-dom/server';
import env from '~/environment';
import AppRoutes from '~/routes';

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

export const queryClientOptions: QueryClientConfig = {
  defaultOptions: {
    queries: {
      staleTime: 10000,
    },
  },
}

const QueryClientContainer: FunctionComponent<{}> = ({ children }) => {
  const [queryClient] = useState(() => new QueryClient(queryClientOptions));
  const [dehydratedState, setDehydratedState] = useState(() => env.isBrowser ? window['__REACT_QUERY_STATE__'] : undefined);

  if (env.isDev) {
    // Dev environment does not require hydration
    return (
      <QueryClientProvider client={queryClient}>
        {children}
        <ReactQueryDevtools />
      </QueryClientProvider>
    );
  } else if (env.isBrowser) {
    // Unable to retrieve window state prerendered by server without setTimeout
    setTimeout(() => {
      if (!dehydratedState) {
        // setDehydratedState(window['__REACT_QUERY_STATE__']);
      }
    }, 0);

    // Client environment will get dehydrated state from __REACT_QUERY_STATE__, which is prerendered by server
    return (
      <QueryClientProvider client={queryClient}>
        <Hydrate state={dehydratedState}>
          {children}
        </Hydrate>
      </QueryClientProvider>
    );
  } else {
    // Server environment's QueryClientProvider will be rendered on ReactJS.NET side
    return <>{children}</>;
  }
}

export const App: FunctionComponent<AppAndRoutingProps> = (props) => (
  <QueryClientContainer>
    <Routing location={props.location} />
  </QueryClientContainer>
);
