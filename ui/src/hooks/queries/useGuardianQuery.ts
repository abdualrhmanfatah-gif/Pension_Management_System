import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";
import type { GuardianDto } from "@/types/retirement";

export const GUARDIAN_KEY = "guardian";

export function useGuardianQuery(id: number | undefined, enabled = true) {
    return useQuery({
        queryKey: [GUARDIAN_KEY, id],
        queryFn: () => retirementApi.getGuardianById(id!) as Promise<GuardianDto>,
        enabled: enabled && !!id && id > 0,
        staleTime: 10_000,
    });
}
