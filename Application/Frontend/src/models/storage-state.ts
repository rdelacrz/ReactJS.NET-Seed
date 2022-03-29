export interface State {
  authToken?: string;
  test?: number;
}

export type StateType = 'authToken' | 'test';

export const initialState: State = {

}
