import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";

interface FileEntity {
    value: string;
    label: string;
}

/**
 * جلب كيانات الملف (متقاعد + وكلاء + أوصياء + مستحقين) — hook واحد.
 */
export function useFileEntitiesQuery(fileId: number | undefined, enabled = true) {
    return useQuery({
        queryKey: ["file-entities", fileId],
        queryFn: async (): Promise<FileEntity[]> => {
            if (!fileId) return [];

            const [agents, guardians, entitled] = await Promise.all([
                retirementApi.getAgents(fileId).catch(() => []),
                retirementApi.getGuardians(fileId).catch(() => []),
                retirementApi.getEntitledPersons(fileId).catch(() => []),
            ]);

            const entities: FileEntity[] = [
                { value: "retiree", label: "المتقاعد (صاحب الملف)" },
            ];

            agents.forEach((a: any) =>
                entities.push({ value: `agent_${a.agentID}`, label: `الوكيل: ${a.fullName}` })
            );
            guardians.forEach((g: any) =>
                entities.push({ value: `guardian_${g.guardianID}`, label: `الوصي: ${g.fullName}` })
            );
            entitled.forEach((e: any) =>
                entities.push({ value: `entitled_${e.entitledID}`, label: `المستفيد: ${e.fullName}` })
            );

            return entities;
        },
        enabled: enabled && !!fileId,
        staleTime: 5 * 60 * 1000,
    });
}
