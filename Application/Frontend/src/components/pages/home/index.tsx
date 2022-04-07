import React, { FunctionComponent, useEffect } from 'react';
import { useQuery } from 'react-query';
import { configService } from '~/services';
import { usePersistentState } from '~/utilities';

import { ReactComponent as UserIcon } from '~/assets/icons/user-icon.svg';

import './styles.scss';

export const HomePage: FunctionComponent<{}> = () => {
  const [authToken, setAuthToken] = usePersistentState<string>('authToken');
  const { data: options, isFetched } = useQuery('options', () => configService.getLookupData('agencyType'));

  const testImg = require('~/assets/graphics/public-docs-img.png');

  console.log('getLookupData', options)
  useEffect(() => {
    setAuthToken('testAuth');
  }, [])
  return (
    <div>
      Home Page, SessionStore Auth Token: {'authToken'} <UserIcon />
      <img src={testImg} alt='Test Image' />
    </div>
  );
}

export default HomePage;
