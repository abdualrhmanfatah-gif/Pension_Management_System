import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { fiscalYearsApi } from "@/api/accounting";

export const FISCAL_YEARS_KEY = "fiscal-years" as const;

export function useFiscalYearsQuery() {
  return useQuery({
    queryKey: [FISCAL_YEARS_KEY],
    queryFn: () => fiscalYearsApi.getAll(),
  });
}
