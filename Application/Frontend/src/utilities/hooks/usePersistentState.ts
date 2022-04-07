import { useMutation, useQuery, UseMutateAsyncFunction, useQueryClient } from 'react-query';
import env from '~/environment';
import { initialState, StateType } from '~/models';

const getStorageData = <T extends any>(key: StateType) => {
  let parsedData = initialState[key];
  if (env.isBrowser) {
    const rawData = sessionStorage.getItem(key);
    parsedData = rawData ? JSON.parse(rawData) : initialState[key];
  }
  return parsedData as T;
}

export const usePersistentState = <T>(key: StateType) => {
  const queryClient = useQueryClient();

  const queryKey = `sessionStorage_${key}`;
  const mutationKey = `sessionStorage_${key}_mutation`;

  const { data } = useQuery(queryKey, () => getStorageData<T>(key));

  const { mutateAsync: setData } = useMutation(
    mutationKey,
    async (value: T) => {
      if (env.isBrowser) {
        sessionStorage.setItem(key, JSON.stringify(value));
      }
      return value;
    },
    {
      onMutate: (mutatedData) => {
        const current = data;
        queryClient.setQueryData(queryKey, mutatedData);
        return current;
      },
      onError: (_, __, rollback) => {
        queryClient.setQueryData(queryKey, rollback);
      },
    }
  );

  return [data, setData] as [T, UseMutateAsyncFunction<T, unknown, T, unknown>];
}

export default usePersistentState;