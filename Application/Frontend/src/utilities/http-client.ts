import axios from 'axios';
import env from 'environment';
import { GraphQLClient, RequestDocument } from 'graphql-request';

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

const graphQLClient = new GraphQLClient(
  env.apiPath + '/graphql',
  { headers });

export const gqlRequest = async <T, V>(query: RequestDocument, variables?: V) => {
  graphQLClient.request<T, V>(query, variables)
};
