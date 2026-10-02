import { documentsApi } from "@/api/documents";
import type { MissingDocumentSummary } from "@/types/retirement";

export interface MissingDocOption {
    id: number;
    name: string;
    entityName?: string | null;
    entitledID?: number | null;
    agentID?: number | null;
    guardianID?: number | null;
    entityType?: string | null;
}

export function mapMissingDocsForDialog(docs: MissingDocumentSummary[]): MissingDocOption[] {
    return docs.map((d) => ({
        id: d.missingDocumentID || d.documentTypeID,
        name: d.documentTypeName,
        entityName: d.entityName,
        entitledID: d.entitledID,
        agentID: d.agentID,
        guardianID: d.guardianID,
        entityType: d.entityType,
    }));
}

export async function resolveMissingDocumentIds(
    selected: (string | number)[],
    missingDocs: MissingDocumentSummary[],
    fileId: number
): Promise<number[]> {
    const result: number[] = [];
    for (const id of selected) {
        const doc = missingDocs.find((d) => d.missingDocumentID === id || d.documentTypeID === id);
        if (doc) {
            if (doc.missingDocumentID > 0) {
                result.push(doc.missingDocumentID);
            } else {
                try {
                    const missingId = await documentsApi.createMissing({
                        fileID: fileId,
                        documentTypeID: doc.documentTypeID,
                    });
                    result.push(missingId);
                } catch {
                    console.error("Failed to create missing document for type", doc.documentTypeID);
                }
            }
        }
    }
    return result;
}
