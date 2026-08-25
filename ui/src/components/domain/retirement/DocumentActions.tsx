import { useState } from "react";
import { Download, Pencil, StickyNote, Settings, Trash2 } from "lucide-react";
import { Button } from "@/components/ui";
import { useHasPermission } from "@/stores/authStore";
import { useApiMutation, useConfirm } from "@/hooks";
import { useQueryClient } from "@tanstack/react-query";
import { documentsApi } from "@/api/documents";
import api, { getFileUrl } from "@/api/axios";
import { cn } from "@/lib/utils";
import { EditDocumentDialog } from "./EditDocumentDialog";
import { NoteDialog } from "./NoteDialog";
import { DocumentProcessDialog } from "./DocumentProcessDialog";
import type { DocumentItem } from "@/types/retirement";

interface DocumentActionsProps {
    document: DocumentItem;
    fileId: number;
    /** Which actions to show. Defaults to all available actions. */
    actions?: ("download" | "edit" | "note" | "process" | "delete")[];
    /** Callback after any mutation succeeds */
    onRefresh?: () => void;
    /** Size of the icon buttons */
    size?: "sm" | "md" | "lg";
    /** Show labels next to icons (instead of icon-only) */
    showLabels?: boolean;
}

export function DocumentActions({
    document: doc,
    fileId,
    actions = ["download", "edit", "note", "process"],
    onRefresh,
    size = "md",
    showLabels = false,
}: DocumentActionsProps) {
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();

    const canEdit = useHasPermission("File.Documents.Update");
    const canDelete = useHasPermission("File.Delete.Soft");

    const [editOpen, setEditOpen] = useState(false);
    const [noteOpen, setNoteOpen] = useState(false);
    const [processOpen, setProcessOpen] = useState(false);

    const deleteMutation = useApiMutation(
        async (docId: number) => documentsApi.delete(docId),
        {
            successMessage: "تم حذف الوثيقة بنجاح",
            invalidateKeys: [["documents", fileId], ["missing-documents", fileId]],
            onSuccess: () => onRefresh?.(),
        }
    );

    const noteMutation = useApiMutation(
        async ({ docId, notes, documentTypeID }: { docId: number; notes: string; documentTypeID: number }) => {
            const formData = new FormData();
            formData.append("DocumentTypeID", documentTypeID.toString());
            formData.append("Notes", notes);
            return documentsApi.update(docId, formData);
        },
        {
            successMessage: "تم حفظ الملاحظة بنجاح",
            invalidateKeys: [["documents", fileId]],
            onSuccess: () => onRefresh?.(),
        }
    );

    const handleDownload = async () => {
        if (!doc.filePath) return;
        const url = getFileUrl(doc.filePath);
        if (!url) return;
        const res = await api.get(url, { responseType: "blob" });
        const blob = new Blob([res.data], { type: res.headers["content-type"] });
        const blobUrl = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = blobUrl;
        a.download = doc.documentTypeName || "document";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(blobUrl);
    };

    const handleDelete = async () => {
        const ok = await confirm({
            title: "تأكيد الحذف",
            description: `هل أنت متأكد من حذف وثيقة "${doc.documentTypeName}"؟`,
            variant: "danger",
        });
        if (ok) {
            await deleteMutation.mutateAsync(doc.documentID);
        }
    };

    const iconSize = size === "sm" ? "w-3.5 h-3.5" : size === "lg" ? "w-5 h-5" : "w-4 h-4";
    const btnSize = size === "sm" ? "h-7 w-7" : size === "lg" ? "h-10 w-10" : "h-8 w-8";

    return (
        <>
            <div className={cn("flex items-center gap-1", showLabels && "flex-wrap")}>
                {/* Download */}
                {actions.includes("download") && doc.filePath && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className={btnSize}
                        onClick={handleDownload}
                        title="تحميل"
                    >
                        <Download className={iconSize} />
                        {showLabels && <span className="mr-1">تحميل</span>}
                    </Button>
                )}

                {/* Edit */}
                {actions.includes("edit") && canEdit && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className={btnSize}
                        onClick={() => setEditOpen(true)}
                        title="تعديل"
                    >
                        <Pencil className={iconSize} />
                        {showLabels && <span className="mr-1">تعديل</span>}
                    </Button>
                )}

                {/* Note */}
                {actions.includes("note") && canEdit && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className={cn(btnSize, doc.notes ? "text-yellow-600" : "")}
                        onClick={() => setNoteOpen(true)}
                        title={doc.notes ? "تعديل الملاحظة" : "إضافة ملاحظة"}
                    >
                        <StickyNote className={iconSize} />
                        {showLabels && <span className="mr-1">ملاحظة</span>}
                    </Button>
                )}

                {/* Process */}
                {actions.includes("process") && canEdit && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className={btnSize}
                        onClick={() => setProcessOpen(true)}
                        title="معالجة"
                    >
                        <Settings className={iconSize} />
                        {showLabels && <span className="mr-1">معالجة</span>}
                    </Button>
                )}

                {/* Delete */}
                {actions.includes("delete") && canDelete && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className={cn(btnSize, "text-destructive")}
                        onClick={handleDelete}
                        title="حذف"
                    >
                        <Trash2 className={iconSize} />
                        {showLabels && <span className="mr-1">حذف</span>}
                    </Button>
                )}
            </div>

            {/* Dialogs */}
            <EditDocumentDialog
                open={editOpen}
                onOpenChange={setEditOpen}
                document={doc}
                fileId={fileId}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
                    setEditOpen(false);
                    onRefresh?.();
                }}
            />

            <NoteDialog
                open={noteOpen}
                onOpenChange={setNoteOpen}
                onSubmit={async (values) => {
                    await noteMutation.mutateAsync({
                        docId: doc.documentID,
                        notes: values.content,
                        documentTypeID: doc.documentTypeID,
                    });
                    setNoteOpen(false);
                }}
                isLoading={noteMutation.isPending}
                initialData={doc.notes ? { content: doc.notes } : undefined}
            />

            <DocumentProcessDialog
                open={processOpen}
                onOpenChange={setProcessOpen}
                document={{
                    documentID: doc.documentID,
                    documentTypeID: doc.documentTypeID,
                    documentTypeName: doc.documentTypeName,
                    fileName: doc.fileName,
                    filePath: doc.filePath ?? undefined,
                    notes: doc.notes ?? undefined,
                    uploadedByUser: doc.uploadedByUser ?? undefined,
                    createdAt: doc.createdAt,
                }}
                fileId={fileId}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
                    setProcessOpen(false);
                    onRefresh?.();
                }}
            />

            {ConfirmDialogElement}
        </>
    );
}
