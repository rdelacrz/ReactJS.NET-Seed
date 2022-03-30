import { useEffect } from 'react';
import env from '~~/environment';

export const useDocTitle = (docTitle: string) => {
  useEffect(() => {
    if (env.isBrowser) {
      document.title = docTitle;
    }
  });
}
