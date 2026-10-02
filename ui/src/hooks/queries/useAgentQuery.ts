import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";
import type { AgentDto } from "@/types/retirement";

export const AGENT_KEY = "agent";

export function useAgentQuery(id: number | undefined, enabled = true) {
    return useQuery({
        queryKey: [AGENT_KEY, id],
        queryFn: () => retirementApi.getAgentById(id!) as Promise<AgentDto>,
        enabled: enabled && !!id && id > 0,
        staleTime: 10_000,
    });
}
