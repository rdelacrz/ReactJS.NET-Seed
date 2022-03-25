import '~~/polyfills';
import React, { FunctionComponent } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { StaticRouter } from 'react-router-dom/server';
import AppRoutes from '~~/routes';

import './app.scss';

interface AppProps {
  location?: string;
}

const App: FunctionComponent<AppProps> = (props) => {
  return (
    typeof window !== 'undefined' ? (
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    ) : (
      <StaticRouter location={props.location}>
        <AppRoutes />
      </StaticRouter>
    )
  );
}

export default App;
