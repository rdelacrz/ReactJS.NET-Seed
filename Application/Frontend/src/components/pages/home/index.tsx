import React, { FunctionComponent, useEffect } from 'react';
import { configService } from '~~/services';
import { useQuery, usePersistentState } from '~~/utilities';

import './styles.scss';

export const HomePage: FunctionComponent<{}> = () => {
  const [authToken, setAuthToken] = usePersistentState<string>('authToken');
  const { data: options, isFetched } = useQuery('options', () => configService.getLookupData('agencyType'));
  useEffect(() => {
    setAuthToken('testAuth');
  }, [])
  return (
    <div>Home Page, SessionStore Auth Token: {'authToken'}</div>
  );
}

export default HomePage;
