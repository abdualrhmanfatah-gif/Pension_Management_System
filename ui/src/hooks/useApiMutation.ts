import { type QueryKey,useMutation, type UseMutationOptions, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { getErrorTitle, normalizeApiError } from "@/lib/errorUtils";

export interface UseApiMutationOptions<TData, TVariables, TError = unknown, TContext = unknown> 
    extends Omit<UseMutationOptions<TData, TError, TVariables, TContext>, 'mutationFn'> {
    /**
     * Query keys to invalidate on success
     */
    invalidateKeys?: QueryKey[];
    /**
     * Success message to show in toast. 
     * If a function is provided, it will be called with the mutation result.
     */
    successMessage?: string | ((data: TData, variables: TVariables) => string);
    /**
     * Set to true to suppress the success toast for silent operations.
     */
    disableSuccessToast?: boolean;
    /**
     * Error message title. Defaults to "خطأ".
     * If not set, the title is auto-detected from the error type:
     *   400 → "خطأ في البيانات المُدخلة"
     *   401/403 → "خطأ في الصلاحيات"
     *   404 → "العنصر غير موجود"
     *   409 → "تعارض في البيانات"
     *   500+ → "خطأ في الخادم"
     *   Network → "خطأ في الاتصال"
     */
    errorTitle?: string;
    /**
     * Callback to close a modal or perform other UI cleanup on success
     */
    onSuccessCallback?: (data: TData, variables: TVariables) => void;
}

/**
 * A wrapper around TanStack useMutation that automatically handles:
 * 1. Query invalidation
 * 2. Success/Error toasts
 * 3. Standard error message extraction
 */
export function useApiMutation<TData, TVariables, TError = unknown, TContext = unknown>(
    mutationFn: (variables: TVariables) => Promise<TData>,
    options: UseApiMutationOptions<TData, TVariables, TError, TContext> = {}
) {
    const queryClient = useQueryClient();
    const { 
        invalidateKeys, 
        successMessage, 
        disableSuccessToast = false,
        errorTitle,
        onSuccessCallback,
        onSuccess,
        onError,
        ...mutationOptions 
    } = options;

    return useMutation<TData, TError, TVariables, TContext>({
        mutationFn,
        onSuccess: async (data, variables, _onMutateResult, _context) => {
            await queryClient.invalidateQueries();
            // 1. Invalidate Queries
            if (invalidateKeys && invalidateKeys.length > 0) {
                await Promise.all(
                    invalidateKeys.map(key => queryClient.invalidateQueries({ queryKey: key }))
                );
            }

            // 2. Show Success Toast (unless suppressed)
            if (!disableSuccessToast && successMessage) {
                const msg = typeof successMessage === 'function' 
                    ? successMessage(data, variables) 
                    : successMessage;
                toast.success(msg);
            }

            // 3. Call Callbacks
            onSuccessCallback?.(data, variables);
            onSuccess?.(data, variables, _onMutateResult, _context);
        },
        onError: (error, variables, _onMutateResult, _context) => {
            // 1. Extract Error Message
            const { message: description } = normalizeApiError(error);

            // 2. Show Error Toast with auto-detected or explicit title
            const title = errorTitle || getErrorTitle(error);
            toast.error(title, { description });

            // 3. Call Callback
            onError?.(error, variables, _onMutateResult, _context);
        },
        ...mutationOptions
    });
}
