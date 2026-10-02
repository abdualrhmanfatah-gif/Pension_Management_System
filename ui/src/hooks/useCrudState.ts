import { useCallback,useState } from "react";

/**
 * Custom hook to manage common CRUD (Create, Read, Update, Delete) dialog states.
 * Eliminates repetitive boilerplate for opening/closing modals and tracking the selected item.
 * 
 * @template T - The DTO/Entity type being managed.
 */
export interface UseCrudStateReturn<T> {
    onOpenChange: (open: boolean) => void;
    // Create/Edit form state
    isFormOpen: boolean;
    selectedItem: T | null;
    isEditing: boolean;
    openCreate: () => void;
    openEdit: (item: T) => void;
    closeForm: () => void;
    
    // Delete confirmation state
    deleteTarget: T | null;
    isDeleteOpen: boolean;
    openDelete: (item: T) => void;
    closeDelete: () => void;
    
    // Documents/Viewer state
    viewerTarget: T | null;
    isViewerOpen: boolean;
    openViewer: (item: T) => void;
    closeViewer: () => void;
}

export function useCrudState<T>(): UseCrudStateReturn<T> {
    // 1. Create/Edit State
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [selectedItem, setSelectedItem] = useState<T | null>(null);

    const openCreate = useCallback(() => {
        setSelectedItem(null);
        setIsFormOpen(true);
    }, []);

    const openEdit = useCallback((item: T) => {
        setSelectedItem(item);
        setIsFormOpen(true);
    }, []);

    const closeForm = useCallback(() => {
        setIsFormOpen(false);
        setSelectedItem(null);
    }, []);

    const onOpenChange = useCallback((open: boolean) => {
        if (!open) {
            setIsFormOpen(false);
            setSelectedItem(null);
        }
    }, []);

    // 2. Delete State
    const [deleteTarget, setDeleteTarget] = useState<T | null>(null);
    const isDeleteOpen = deleteTarget !== null;

    const openDelete = useCallback((item: T) => {
        setDeleteTarget(item);
    }, []);

    const closeDelete = useCallback(() => {
        setDeleteTarget(null);
    }, []);

    // 3. Viewer State
    const [viewerTarget, setViewerTarget] = useState<T | null>(null);
    const isViewerOpen = viewerTarget !== null;

    const openViewer = useCallback((item: T) => {
        setViewerTarget(item);
    }, []);

    const closeViewer = useCallback(() => {
        setViewerTarget(null);
    }, []);

    return {
        onOpenChange,
        isFormOpen,
        selectedItem,
        isEditing: selectedItem !== null,
        openCreate,
        openEdit,
        closeForm,
        
        deleteTarget,
        isDeleteOpen,
        openDelete,
        closeDelete,
        
        viewerTarget,
        isViewerOpen,
        openViewer,
        closeViewer,
    };
}
