import React, { FunctionComponent } from 'react';
import { Routes, Route } from 'react-router-dom';
import { HomePage } from '~/components/pages/home';
import { TestPage } from '~/components/pages/test';

const AppRoutes: FunctionComponent<{}> = () => (
  <Routes>
    <Route path='/' element={<HomePage />} />
    <Route path='/test' element={<TestPage />} />
  </Routes>
)

export default AppRoutes;