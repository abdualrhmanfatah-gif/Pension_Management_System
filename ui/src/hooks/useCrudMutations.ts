import { type QueryKey } from "@tanstack/react-query";

import { useApiMutation, type UseApiMutationOptions } from "./useApiMutation";

export interface UseCrudMutationsOptions<TResponse, TCreateVars, TUpdateVars, TDeleteVars = number> {
    entityName: string;
    baseQueryKeys: QueryKey[];
    createFn?: (vars: TCreateVars) => Promise<TResponse>;
    updateFn?: (vars: TUpdateVars) => Promise<TResponse>;
    deleteFn?: (vars: TDeleteVars) => Promise<TResponse>;
    onSuccess?: () => void;
    createOptions?: Omit<UseApiMutationOptions<TResponse, TCreateVars>, 'mutationFn'>;
    updateOptions?: Omit<UseApiMutationOptions<TResponse, TUpdateVars>, 'mutationFn'>;
    deleteOptions?: Omit<UseApiMutationOptions<TResponse, TDeleteVars>, 'mutationFn'>;
}

/**
 * A convenience hook for standard CRUD mutations
 */
export function useCrudMutations<TResponse, TCreateVars, TUpdateVars, TDeleteVars = number>(
    options: UseCrudMutationsOptions<TResponse, TCreateVars, TUpdateVars, TDeleteVars>
) {
    const { 
        entityName, 
        baseQueryKeys, 
        createFn, 
        updateFn, 
        deleteFn, 
        onSuccess,
        createOptions,
        updateOptions,
        deleteOptions
    } = options;

    const createMutation = useApiMutation(createFn!, {
        invalidateKeys: baseQueryKeys,
        successMessage: `تم إنشاء ${entityName} بنجاح`,
        onSuccessCallback: onSuccess,
        ...createOptions
    });

    const updateMutation = useApiMutation(updateFn!, {
        invalidateKeys: baseQueryKeys,
        successMessage: `تم تحديث ${entityName} بنجاح`,
        onSuccessCallback: onSuccess,
        ...updateOptions
    });

    const deleteMutation = useApiMutation(deleteFn!, {
        invalidateKeys: baseQueryKeys,
        successMessage: `تم حذف ${entityName} بنجاح`,
        onSuccessCallback: onSuccess,
        ...deleteOptions
    });

    return {
        createMutation,
        updateMutation,
        deleteMutation
    };
}
