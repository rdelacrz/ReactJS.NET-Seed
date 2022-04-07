import { useLayoutEffect, useState } from 'react';
import env from '~/environment';

export const useWindowSize = () => {
  const [size, setSize] = useState([0, 0]);
  if (env.isBrowser) {
    useLayoutEffect(() => {
      function updateSize() {
        setSize([window.innerWidth, window.innerHeight]);
      }
      window.addEventListener('resize', updateSize);
      updateSize();
      return () => window.removeEventListener('resize', updateSize);
    }, []);
  }
  return {width: size[0], height: size[1]};
}
