import { useQuery } from "@tanstack/react-query";

import { documentsApi } from "@/api/documents";
import type { MissingDocumentSummary } from "@/types/retirement";

export const MISSING_DOCUMENTS_KEY = "missing-documents";

export function useMissingDocumentsQuery(fileId: number) {
    return useQuery({
        queryKey: [MISSING_DOCUMENTS_KEY, fileId],
        queryFn: async () => {
            const [manual, autoDetected] = await Promise.all([
                documentsApi.getMissingByFileId(fileId).catch(() => [] as any[]),
                documentsApi.getAutoDetectedMissing(fileId).catch(() => [] as any[]),
            ]);

            const auto: MissingDocumentSummary[] = autoDetected.map((ad: any) => ({
                missingDocumentID: 0,
                documentTypeID: ad.documentTypeID,
                documentTypeName: ad.documentTypeName,
                isAutoDetected: true,
                entityType: ad.entityType,
                entityName: ad.entityName,
                status: "Pending",
                createdAt: new Date().toISOString(),
            }));

            return [...manual, ...auto];
        },
        enabled: !!fileId && fileId > 0,
    });
}
