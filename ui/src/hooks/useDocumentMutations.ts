import { documentsApi } from "@/api/documents";

import { useApiMutation } from "./useApiMutation";

const DOCUMENT_KEYS = ["documents"];

/**
 * رفع وثيقة.
 */
export function useUploadDocument(options?: {
    fileId?: number;
    onSuccess?: () => void;
}) {
    return useApiMutation(
        async (formData: FormData) => documentsApi.upload(formData),
        {
            invalidateKeys: options?.fileId
                ? [DOCUMENT_KEYS, [options.fileId], ["missing-documents", options.fileId]]
                : [DOCUMENT_KEYS],
            successMessage: "تم رفع الوثيقة بنجاح",
            errorTitle: "خطأ في رفع الوثيقة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * حذف وثيقة.
 */
export function useDeleteDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async (docId: number) => documentsApi.delete(docId),
        {
            invalidateKeys: [
                [DOCUMENT_KEYS[0], fileId],
                ["missing-documents", fileId],
            ],
            successMessage: "تم حذف الوثيقة بنجاح",
            errorTitle: "خطأ في حذف الوثيقة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * تعديل وثيقة.
 */
export function useUpdateDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async ({ docId, formData }: { docId: number; formData: FormData }) =>
            documentsApi.update(docId, formData),
        {
            invalidateKeys: [[DOCUMENT_KEYS[0], fileId]],
            successMessage: "تم تعديل الوثيقة بنجاح",
            errorTitle: "خطأ في تعديل الوثيقة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * مراجعة وثيقة.
 */
export function useReviewDocument(options?: {
    invalidateKey?: any[];
    onSuccess?: () => void;
}) {
    return useApiMutation(
        async ({
            id,
            dto,
        }: {
            id: number;
            dto: { pendingReviewStatus: number; notes?: string; rejectionReason?: string };
        }) => documentsApi.review(id, dto),
        {
            invalidateKeys: options?.invalidateKey || [DOCUMENT_KEYS],
            successMessage: "تم حفظ نتيجة الفحص بنجاح",
            errorTitle: "خطأ في المراجعة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * تأشير وثيقة ناقصة كمسلّمة.
 */
export function useFulfillMissingDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async (missingId: number) => documentsApi.fulfillMissing(missingId),
        {
            invalidateKeys: [
                [DOCUMENT_KEYS[0], fileId],
                ["missing-documents", fileId],
                ["retirement-file"],
            ],
            successMessage: "تم تأشير الوثيقة كمسلّمة بنجاح",
            errorTitle: "خطأ في التأشير",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * إنشاء وثيقة ناقصة يدوياً.
 */
export function useCreateMissingDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async (data: {
            fileID: number;
            documentTypeID: number;
            agentID?: number;
            guardianID?: number;
            entitledID?: number;
        }) => documentsApi.createMissing(data),
        {
            invalidateKeys: [
                [DOCUMENT_KEYS[0], fileId],
                ["missing-documents", fileId],
            ],
            successMessage: "تم إضافة الوثيقة الناقصة بنجاح",
            errorTitle: "خطأ في الإضافة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}
