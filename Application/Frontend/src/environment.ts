const isBrowser = typeof window !== 'undefined';
const isDev = process.env.NODE_ENV === 'development';
const appPath = process.env.APP_PATH;

const getBuildStatus = () => {
  if (isBrowser) {
    const url = window.location.href;
    if (url && (url.indexOf('dev') > -1 || url.indexOf('localhost') > -1)) {
      // Replace with actual Jenkins icon url once Jenkins dev setup is established
      return 'http://jenkins.inovas.net/buildStatus/icon?job=ERECOGNITION2.0_DEV';
    }
  }
  return;
}

const environment = {
  originalAppPath: appPath,
  appPath: appPath ? appPath + '/#/' : '/',   // Simulates hash browser URL while keeping browser history benefits
  apiPath: isBrowser ? window.location.origin + appPath + '/api/' : '/api/',
  buildStatus: getBuildStatus(),
  isDev,
  isBrowser,
};

export default environment;