import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";

export const GUARDIANS_KEY = "guardians-tab";

export function useGuardiansQuery(fileId: number | undefined) {
    return useQuery({
        queryKey: [GUARDIANS_KEY, fileId],
        queryFn: () => retirementApi.getGuardians(fileId!),
        enabled: !!fileId && fileId > 0,
    });
}
