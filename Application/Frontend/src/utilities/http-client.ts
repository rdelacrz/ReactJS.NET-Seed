import axios from 'axios';
import env from 'environment';

// If authentication exists in application, have this retrieve actual token
const authToken: string = null;

// Sets up headers
const headers = {
  'Accept': 'application/json',
  'Content-Type': 'application/json',
};
if (authToken) {
  headers['Authorization'] = `Bearer ${authToken}`;
}

export const http = axios.create({
  baseURL: env.apiPath,
  headers,
});

type GraphQLResponse<T> = {
  data: T;
};

export const gqlRequest = async <T>(query: string, variables?: { [id: string]: any }) => {
  const gqlBody = JSON.stringify({ query, variables });
  const resp = await http.post<GraphQLResponse<T>>(env.apiPath + '/graphql', gqlBody);
  return resp.data.data;
}