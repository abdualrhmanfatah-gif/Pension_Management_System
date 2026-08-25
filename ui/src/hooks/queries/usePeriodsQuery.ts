import { useQuery } from "@tanstack/react-query";
import { periodsApi } from "@/api/accounting";

export const PERIODS_KEY = "periods" as const;

export function usePeriodsQuery(fiscalYearId: number | undefined | null) {
  return useQuery({
    queryKey: [PERIODS_KEY, fiscalYearId],
    queryFn: () => periodsApi.getAll(fiscalYearId!),
    enabled: !!fiscalYearId,
  });
}
