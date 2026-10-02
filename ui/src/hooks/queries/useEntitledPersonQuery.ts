import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";
import type { EntitledPersonDto } from "@/types/retirement";

export const ENTITLED_PERSON_KEY = "entitled-person";

export function useEntitledPersonQuery(id: number | undefined, enabled = true) {
    return useQuery({
        queryKey: [ENTITLED_PERSON_KEY, id],
        queryFn: () => retirementApi.getEntitledPersonById(id!) as Promise<EntitledPersonDto>,
        enabled: enabled && !!id && id > 0,
        staleTime: 10_000,
    });
}
