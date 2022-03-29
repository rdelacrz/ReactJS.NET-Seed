import '~~/polyfills';
import React, { FunctionComponent } from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ReactQueryDevtools } from 'react-query/devtools'
import { BrowserRouter } from 'react-router-dom';
import { StaticRouter } from 'react-router-dom/server';
import AppRoutes from '~~/routes';

import './app.scss';

const queryClient = new QueryClient();

interface AppAndRoutingProps {
  location?: string;
}

const Routing: FunctionComponent<AppAndRoutingProps> = (props) => (
  typeof window !== 'undefined' ? (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  ) : (
    <StaticRouter location={props.location}>
      <AppRoutes />
    </StaticRouter>
  )
)

const App: FunctionComponent<AppAndRoutingProps> = (props) => (
  <QueryClientProvider client={queryClient}>
    <Routing location={props.location} />
    <ReactQueryDevtools initialIsOpen />
  </QueryClientProvider>
);

export default App;
