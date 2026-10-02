import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, Inbox, Plus, StickyNote, Trash2 } from "lucide-react";
import { useEffect } from "react";
import { toast } from "sonner";

import { retirementApi } from "@/api/retirement";
import { NoteDialog } from "@/components/domain/retirement/NoteDialog";
import { Button, Card, EmptyState, Heading, useConfirm } from "@/components/ui";
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
            <Card className="flex flex-col md:flex-row md:items-center justify-between gap-2 md:gap-4 bg-muted/30 p-3 md:p-5">
                <div className="space-y-0.5 md:space-y-1">
                    <Heading level="h3" className="font-bold flex items-center gap-1.5 md:gap-2 text-sm md:text-base">
                        <StickyNote className="w-4 h-4 md:w-5 md:h-5 text-amber-500" /> ملاحظات الملف
                    </Heading>
                    <p className="text-xs md:text-sm text-muted-foreground hidden sm:block">التوثيق الإداري والملاحظات الهامة المتعلقة بسير معاملة الملف</p>
                </div>
                {canCreate && (
                    <Button size="sm" onClick={crud.openCreate} className="gap-1.5  bg-amber-600 hover:bg-amber-700 h-8 md:h-auto text-xs md:text-sm px-3 md:px-4">
                        <Plus className="w-3.5 h-3.5 md:w-4 md:h-4" /> إضافة ملاحظة
                    </Button>
                )}
            </Card>

            {notes.length === 0 ? (
                <EmptyState icon={Inbox} title="لا توجد ملاحظات مسجلة لهذا الملف" />
            ) : (
                <div className="grid grid-cols-1 gap-3">
                    {notes.map(n => (
                        <div key={n.noteID} className="group border rounded-xl p-5 bg-card  transition-all border-border/60">
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
                                <span className="font-bold text-foreground/70">{n.createdByUsername || "نظام"}</span>
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
