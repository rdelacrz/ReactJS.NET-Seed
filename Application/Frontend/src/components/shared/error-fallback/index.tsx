import React, { FunctionComponent } from 'react';
import { FallbackProps } from 'react-error-boundary';
import env from '~/environment';

export const ErrorFallback: FunctionComponent<FallbackProps> = (props) => {
  return (
    <div role='alert'>
      <p>Something went wrong:</p>
      <pre>{props.error.message}</pre>
      <button onClick={() => env.isBrowser && window.history.back()}>
        Go Back
      </button>
    </div>
  );
}

export default ErrorFallback;
