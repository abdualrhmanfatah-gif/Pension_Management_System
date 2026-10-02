import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";

export const ENTITLED_PERSONS_KEY = "entitled-persons";

export function useEntitledPersonsQuery(fileId: number | undefined) {
    return useQuery({
        queryKey: [ENTITLED_PERSONS_KEY, fileId],
        queryFn: () => retirementApi.getEntitledPersons(fileId!),
        enabled: !!fileId && fileId > 0,
    });
}
