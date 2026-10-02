import { useQuery } from "@tanstack/react-query";

import { pensionStatsApi } from "@/api/pension";

/**
 * Hook لجلب إحصائيات المعاشات
 */
export function usePensionStatistics() {
  return useQuery({
    queryKey: ["pension-statistics"],
    queryFn: () => pensionStatsApi.getStatistics(),
    staleTime: 60000, // دقيقة واحدة
    refetchInterval: 300000, // 5 دقائق
  });
}

export default usePensionStatistics;
