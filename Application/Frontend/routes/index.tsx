import React, { FunctionComponent } from 'react';
import { Routes, Route } from 'react-router-dom';
import { HomePage } from '~~/components/pages/home';

const AppRoutes: FunctionComponent<{}> = () => (
  <Routes>
    <Route path='/' element={<HomePage />} />
  </Routes>
)

export default AppRoutes;