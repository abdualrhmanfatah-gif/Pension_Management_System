import { useQuery } from "@tanstack/react-query";
import { documentsApi } from "@/api/documents";
import type { DocumentItem, MissingDocumentSummary, DocumentTypeLookup } from "@/types/retirement";

export const DOCUMENTS_QUERY_KEY = "documents";

/**
 * جلب الوثائق + الناقصة + الأنواع — hook واحد.
 */
export function useDocumentsQuery(fileId: number) {
    return useQuery({
        queryKey: [DOCUMENTS_QUERY_KEY, fileId],
        queryFn: async () => {
            const [docs, missing, autoDetected, types] = await Promise.all([
                documentsApi.getByFileId(fileId).catch(() => [] as DocumentItem[]),
                documentsApi.getMissingByFileId(fileId).catch(() => [] as any[]),
                documentsApi.getAutoDetectedMissing(fileId).catch(() => [] as any[]),
                documentsApi.getTypes().catch(() => [] as DocumentTypeLookup[]),
            ]);

            const manualMissing = missing.map((m: any) => {
                let entityName = m.entityName;
                let entityType = m.entityType;
                if (!entityName) {
                    if (m.agentName) { entityName = m.agentName; entityType = "Agent"; }
                    else if (m.guardianName) { entityName = m.guardianName; entityType = "Guardian"; }
                    else if (m.entitledName) { entityName = m.entitledName; entityType = "EntitledPerson"; }
                }
                return { ...m, entityName, entityType };
            });

            const autoMissing: MissingDocumentSummary[] = autoDetected.map((ad: any) => ({
                missingDocumentID: 0,
                documentTypeID: ad.documentTypeID,
                documentTypeName: ad.documentTypeName,
                status: "Pending" as const,
                createdAt: new Date().toISOString(),
                isAutoDetected: true,
                entityName: ad.entityName,
                entityType: ad.entityType,
                retireeID: ad.retireeID || undefined,
                agentID: ad.agentID || undefined,
                agentName: ad.entityType === "Agent" ? ad.entityName : undefined,
                guardianID: ad.guardianID || undefined,
                guardianName: ad.entityType === "Guardian" ? ad.entityName : undefined,
                entitledID: ad.entitledID || undefined,
                entitledName: ad.entityType === "EntitledPerson" ? ad.entityName : undefined,
                pledgeID: ad.pledgeID || undefined,
                exemptionID: ad.exemptionID || undefined,
                notes: undefined,
            }));

            return {
                documents: docs,
                missingDocs: [...manualMissing, ...autoMissing],
                docTypes: types,
            };
        },
        enabled: !!fileId && fileId > 0,
    });
}
