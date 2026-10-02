import { useQuery } from "@tanstack/react-query";

import { accountsApi } from "@/api/accounting";

export const ACCOUNTS_KEY = "accounts" as const;

export function useAllAccountsQuery() {
  return useQuery({
    queryKey: [ACCOUNTS_KEY, "all"],
    queryFn: () => accountsApi.getAll(),
  });
}

export function usePostableAccountsQuery() {
  return useQuery({
    queryKey: [ACCOUNTS_KEY, "postable"],
    queryFn: () => accountsApi.getPostable(),
  });
}
