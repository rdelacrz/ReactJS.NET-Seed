import {
  MutationFunction, MutationKey, QueryFunction, QueryKey, UseMutationOptions, UseQueryOptions,
  useMutation as useMutationOriginal, UseMutationResult, useQuery as useQueryOriginal, UseQueryResult
} from 'react-query';
import env from '~~/environment';

export const useQuery = <TQueryFnData = unknown, TError = unknown, TData = TQueryFnData, TQueryKey extends QueryKey = QueryKey>(
  queryKey: TQueryKey,
  queryFn: QueryFunction<TQueryFnData, TQueryKey>,
  options?: Omit<UseQueryOptions<TQueryFnData, TError, TData, TQueryKey>, 'queryKey' | 'queryFn'>
): UseQueryResult<TData, TError> => {
  if (!env.isBrowser) return {} as any;
  return useQueryOriginal(queryKey, queryFn, options);
}

export const useMutation = <TData = unknown, TError = unknown, TVariables = void, TContext = unknown>(
  mutationKey: MutationKey,
  mutationFn?: MutationFunction<TData, TVariables>,
  options?: Omit<UseMutationOptions<TData, TError, TVariables, TContext>, 'mutationKey' | 'mutationFn'>
): UseMutationResult<TData, TError, TVariables, TContext> => {
  if (!env.isBrowser) return {} as any;
  return useMutationOriginal(mutationKey, mutationFn, options);
}
