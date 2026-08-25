import { useQuery } from "@tanstack/react-query";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import type { DocumentItem, MissingDocumentInfo, PledgeItem, PersonDocumentsResult } from "@/types/retirement";

export const PERSON_DOCUMENTS_KEY = "person-documents";

export function usePersonDocumentsQuery(fileId: number, personId: number | null, personType: string | null) {
    return useQuery({
        queryKey: [PERSON_DOCUMENTS_KEY, fileId, personId, personType],
        queryFn: async (): Promise<PersonDocumentsResult> => {
            const [allDocs, allMissing, allPledges] = await Promise.all([
                documentsApi.getByFileId(fileId).catch(() => [] as DocumentItem[]),
                documentsApi.getMissingByFileId(fileId).catch(() => [] as MissingDocumentInfo[]),
                retirementApi.getPledges(fileId).catch(() => [] as PledgeItem[]),
            ]);

            let uploaded: DocumentItem[] = [];
            let missing: MissingDocumentInfo[] = [];
            let pledges: PledgeItem[] = [];

            if (personType === "entitled") {
                uploaded = allDocs.filter((d) => d.entitledID === personId);
                missing = allMissing.filter((m) => m.entitledID === personId);
                pledges = allPledges.filter((p) => p.missingDocuments?.some((md) => md.entitledID === personId));
            } else if (personType === "agent") {
                uploaded = allDocs.filter((d) => d.agentID === personId);
                missing = allMissing.filter((m) => m.agentID === personId);
                pledges = allPledges.filter((p) => p.missingDocuments?.some((md) => md.agentID === personId));
            } else if (personType === "guardian") {
                uploaded = allDocs.filter((d) => d.guardianID === personId);
                missing = allMissing.filter((m) => m.guardianID === personId);
                pledges = allPledges.filter((p) => p.missingDocuments?.some((md) => md.guardianID === personId));
            }

            return { uploaded, missing, pledges };
        },
        enabled: !!fileId && fileId > 0 && !!personId && !!personType,
        staleTime: 30_000,
    });
}
