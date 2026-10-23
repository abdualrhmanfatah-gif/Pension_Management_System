import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Inbox, Plus, StickyNote, Trash2 } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { NoteDialog } from "@/components/domain/retirement/NoteDialog";
import { Button, EmptyState, TabHeader, useConfirm } from "@/components/ui";
import { useApiMutation, useCreateShortcut, useCrudState } from "@/hooks";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Types ─────────────────────────────────────────────

export interface FileNote {
    noteID: number;
    content: string;
    createdByUsername?: string;
    createdAt: string;
}

interface NotesTabProps {
    fileId: number;
}

/**
 * تبويب الملاحظات والتعليقات الإدارية على الملف
 */
export function NotesTab({ fileId }: NotesTabProps) {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canCreate = useHasPermission("FileNote.Add");
    const canDelete = useHasPermission("FileNote.Delete");
    const crud = useCrudState<FileNote>();

    // جلب البيانات
    const { data: notes = [], isLoading, error } = useQuery({
        queryKey: ["file-notes", fileId],
        queryFn: () => retirementApi.getNotes(fileId).then(data => data as unknown as FileNote[])
    });

    // طفرات البيانات
    const createMutation = useApiMutation(
        (values: { fileID: number; content: string }) =>
            retirementApi.createNote(values),
        {
            successMessage: "تمت إضافة الملاحظة بنجاح",
            invalidateKeys: [["file-notes", fileId]],
            onSuccess: () => crud.closeForm()
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => retirementApi.deleteNote(id),
        {
            successMessage: "تم حذف الملاحظة بنجاح",
            invalidateKeys: [["file-notes", fileId]]
        }
    );

    const handleDelete = async (note: FileNote) => {
        const ok = await confirm({
            title: "حذف الملاحظة",
            description: "هل أنت متأكد من رغبتك في حذف هذه الملاحظة نهائياً؟",
            variant: "danger"
        });
        if (ok) deleteMutation.mutate(note.noteID);
    };

    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل الملاحظات");
    }, [error]);

    useCreateShortcut(crud.openCreate, canCreate);

    if (error) return (
        <div className="py-8 text-center text-destructive flex flex-col items-center gap-2">
            <AlertTriangle className="w-8 h-8" />
            <p className="font-bold">حدث خطأ أثناء تحميل الملاحظات</p>
        </div>
    );

    if (isLoading) return <div className="py-8 text-center text-muted-foreground animate-pulse">جاري التحميل...</div>;

    return (
        <div className="space-y-2 md:space-y-4 animate-in fade-in duration-500 text-start">
            <TabHeader
                title="ملاحظات الملف"
                description="التوثيق الإداري والملاحظات الهامة المتعلقة بسير معاملة الملف"
                icon={StickyNote}
                action={{
                    label: "إضافة ملاحظة",
                    icon: Plus,
                    onClick: crud.openCreate,
                    show: canCreate,
                }}
            />

            {notes.length === 0 ? (
                <EmptyState icon={Inbox} title="لا توجد ملاحظات مسجلة لهذا الملف" />
            ) : (
                <div className="grid grid-cols-1 gap-3">
                    {notes.map(n => (
                        <div key={n.noteID} className="group border rounded-xl p-5 bg-card  transition-all border-border">
                            <div className="flex items-start justify-between gap-4">
                                <p className="text-sm text-foreground leading-relaxed whitespace-pre-wrap flex-1">{n.content}</p>
                                {canDelete && (
                                    <Button
                                        size="icon"
                                        variant="ghost"
                                        className="min-h-[44px] min-w-[44px] text-destructive opacity-0 group-hover:opacity-100 transition-opacity"
                                        onClick={() => handleDelete(n)}
                                        title="حذف الملاحظة"
                                    >
                                        <Trash2 className="w-4 h-4" />
                                    </Button>
                                )}
                            </div>
                            <div className="flex items-center gap-2 mt-4 pt-3 border-t border-dashed text-[10px] sm:text-xs text-muted-foreground">
                                <span className="font-bold text-foreground">{n.createdByUsername || "نظام"}</span>
                                <span className="w-1 h-1 rounded-full bg-muted-foreground/30" />
                                <span className="font-mono">{formatDate(n.createdAt, true)}</span>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {crud.isFormOpen && (
                <NoteDialog
                    open={crud.isFormOpen}
                    onClose={crud.closeForm}
                    onSubmit={(values: { content: string }) => createMutation.mutate({ fileID: fileId, content: values.content })}
                    isLoading={createMutation.isPending}
                />
            )}

            {ConfirmDialogElement}
        </div>
    );
}
