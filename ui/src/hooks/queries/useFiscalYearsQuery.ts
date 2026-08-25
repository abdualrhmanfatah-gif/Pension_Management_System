import { useQuery } from "@tanstack/react-query";
import { fiscalYearsApi } from "@/api/accounting";

export const FISCAL_YEARS_KEY = "fiscal-years" as const;

export function useFiscalYearsQuery() {
  return useQuery({
    queryKey: [FISCAL_YEARS_KEY],
    queryFn: () => fiscalYearsApi.getAll(),
  });
}
