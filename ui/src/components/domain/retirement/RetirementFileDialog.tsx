import { CreateFileWizard } from "@/components/domain/retirement/CreateFileWizard";
import { type BaseOverlayProps } from "@/components/ui";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";

interface RetirementFileDialogProps extends BaseOverlayProps {
    mode?: "create" | "edit";
    fileId?: number;
    retireeId?: number;
    retireeName?: string;
    onSuccess: () => void;
    onCreated?: (fileId: number) => void;
}

/**
 * Unified dialog for creating or editing a retirement file.
 * Wraps the CreateFileWizard component.
 */
export function RetirementFileDialog({
    open,
    onOpenChange,
    mode = "create",
    fileId,
    retireeId,
    retireeName,
    onSuccess,
    onCreated
}: RetirementFileDialogProps) {
    const isEditing = mode === "edit";
    
    const title = isEditing ? "تعديل بيانات ملف التقاعد" : "فتح ملف تقاعد جديد";
    const description = retireeName 
        ? `${isEditing ? "تحديث" : "فتح"} ملف تقاعد للمتقاعد: ${retireeName}. أكمل الخطوات التالية.`
        : isEditing 
            ? "تحديث بيانات ملف التقاعد المختار."
            : "قم بإدخال بيانات ملف التقاعد الجديد في الخطوات التالية.";

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={title}
            description={description}
            size="full"
            className="p-0"
            footer={null} 
        >
            <CreateFileWizard
                mode={mode}
                fileId={fileId}
                initialRetireeId={retireeId}
                initialRetireeName={retireeName}
                onCreated={onCreated}
                onSuccess={() => {
                    onOpenChange?.(false);
                    onSuccess();
                }}
                onCancel={() => onOpenChange?.(false)}
            />
        </BaseFormDialog>
    );
}
