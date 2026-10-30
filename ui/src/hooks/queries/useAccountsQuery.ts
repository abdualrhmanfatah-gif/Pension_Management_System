import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
