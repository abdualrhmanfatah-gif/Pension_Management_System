import { useQuery } from "@tanstack/react-query";

import { retirementApi } from "@/api/retirement";
import type { FileReviewItem, ReviewSectionData } from "@/types/retirement";

export const REVIEW_ITEMS_KEY = "review-items";

function groupBySection(items: FileReviewItem[]): ReviewSectionData[] {
    const map = new Map<string, ReviewSectionData>();

    for (const item of items) {
        const key = item.sectionName || "أخرى";
        if (!map.has(key)) {
            map.set(key, {
                sectionName: key,
                sectionType: item.sectionType || "",
                items: [],
                approvedCount: 0,
                rejectedCount: 0,
                pendingCount: 0,
            });
        }
        const section = map.get(key)!;
        section.items.push(item);
        if (item.status === 1) section.approvedCount++;
        else if (item.status === 2) section.rejectedCount++;
        else section.pendingCount++;
    }

    return Array.from(map.values());
}

export function useReviewItemsQuery(fileId: number) {
    return useQuery({
        queryKey: [REVIEW_ITEMS_KEY, fileId],
        queryFn: async () => {
            const diff = await retirementApi.getReviewDiff(fileId);

            const allItems: FileReviewItem[] = [];
            for (const section of diff.sections || []) {
                for (const change of section.fieldChanges || []) {
                    allItems.push({
                        itemId: change.itemId ?? (change as any).ItemId ?? 0,
                        retirementFileID: fileId,
                        fieldName: change.fieldName ?? (change as any).FieldName ?? "",
                        oldValue: change.oldValue ?? (change as any).OldValue ?? null,
                        newValue: change.newValue ?? (change as any).NewValue ?? null,
                        sectionName: section.sectionName ?? (section as any).SectionName ?? "",
                        sectionType: section.sectionType ?? (section as any).SectionType ?? "",
                        entityDisplayName: section.entityDisplayName ?? (section as any).EntityDisplayName,
                        fieldChangesJson: change.fieldChangesJson ?? (change as any).FieldChangesJson,
                        status: change.itemStatus ?? (change as any).ItemStatus ?? 0,
                        reviewedAt: null,
                        reviewNotes: null,
                    });
                }
            }

            return {
                diff,
                items: allItems,
                sections: groupBySection(allItems),
                totalCount: allItems.length,
                pendingCount: allItems.filter((i) => i.status === 0).length,
                approvedCount: allItems.filter((i) => i.status === 1).length,
                rejectedCount: allItems.filter((i) => i.status === 2).length,
            };
        },
        enabled: !!fileId && fileId > 0,
        staleTime: 10_000,
    });
}
