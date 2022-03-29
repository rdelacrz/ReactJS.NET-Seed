import axios from 'axios';
import env from 'environment';
import { request, RequestDocument } from 'graphql-request';

class HttpSetup {
  readonly baseHeaders = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
  };

  get authToken(): string {
    return null;    // If authentication exists in application, have this retrieve actual token
  }

  private createHttpClient() {
    // Adds authorization token, if currently available
    const headers = { ...this.baseHeaders };
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }

    return axios.create({
      baseURL: env.apiPath,
      headers,
    });
  }

  get client() {
    return this.createHttpClient();
  }
}

export const http = new HttpSetup();

export const gqlRequest = async <T, V>(query: RequestDocument, variables?: V) => (
  request<T, V>(env.apiPath, query, variables)
)
