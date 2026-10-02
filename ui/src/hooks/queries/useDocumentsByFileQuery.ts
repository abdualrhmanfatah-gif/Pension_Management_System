import { useQuery } from "@tanstack/react-query";

import { documentsApi } from "@/api/documents";

export const DOCUMENTS_BY_FILE_KEY = "documents";

/**
 * جلب جميع الوثائق المرفقة بملف تقاعدي.
 * يستبدل النمط المكرر: useQuery({ queryKey: ["documents", fileId], ... })
 */
export function useDocumentsByFileQuery(fileId: number) {
    return useQuery({
        queryKey: [DOCUMENTS_BY_FILE_KEY, fileId],
        queryFn: () => documentsApi.getByFileId(fileId),
        enabled: !!fileId && fileId > 0,
        staleTime: 30_000,
    });
}
