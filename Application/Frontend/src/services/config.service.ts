import { gqlRequest } from '~/utilities';

export const configService = {
  getLookupData: (fieldName: string, activeOnly = true) => gqlRequest(`
    query getLookupData($fieldName: String!, $activeOnly: Boolean) {
      options {
        lookups(fieldName: $fieldName, activeOnly: $activeOnly) {
          valueCode
          valueName
          orderSequence
        }
      }
    }`,
    { fieldName, activeOnly }
  ),
};
