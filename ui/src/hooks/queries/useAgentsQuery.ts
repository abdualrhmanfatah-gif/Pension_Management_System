import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";

export const AGENTS_KEY = "agents";

export function useAgentsQuery(fileId: number | undefined) {
    return useQuery({
        queryKey: [AGENTS_KEY, fileId],
        queryFn: () => retirementApi.getAgents(fileId!),
        enabled: !!fileId && fileId > 0,
    });
}
