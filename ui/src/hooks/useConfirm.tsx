import { useState } from "react";

import { ConfirmDialog } from "@/components/ui/ConfirmDialog";

interface UseConfirmReturn {
    confirm: (options: {
        title: string;
        description?: string;
        confirmLabel?: string;
        cancelLabel?: string;
        variant?: "danger" | "warning" | "default";
    }) => Promise<boolean>;
    ConfirmDialogElement: React.ReactNode;
}

export function useConfirm(): UseConfirmReturn {
    const [open, setOpen] = useState(false);
    const [options, setOptions] = useState<{
        title: string;
        description?: string;
        confirmLabel?: string;
        cancelLabel?: string;
        variant?: "danger" | "warning" | "default";
    }>({ title: "" });
    const [resolver, setResolver] = useState<((value: boolean) => void) | null>(null);

    const confirm = (opts: typeof options): Promise<boolean> => {
        setOptions(opts);
        setOpen(true);
        return new Promise<boolean>((resolve) => {
            setResolver(() => resolve);
        });
    };

    const handleConfirm = () => {
        resolver?.(true);
        setOpen(false);
    };

    const handleCancel = (isOpen: boolean) => {
        if (!isOpen) {
            resolver?.(false);
        }
        setOpen(isOpen);
    };

    const ConfirmDialogElement = (
        <ConfirmDialog
            open={open}
            onOpenChange={handleCancel}
            title={options.title}
            description={options.description}
            confirmLabel={options.confirmLabel}
            cancelLabel={options.cancelLabel}
            variant={options.variant}
            onConfirm={handleConfirm}
        />
    );

    return { confirm, ConfirmDialogElement };
}
