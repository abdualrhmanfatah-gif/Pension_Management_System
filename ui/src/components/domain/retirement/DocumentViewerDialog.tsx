import { FileWarning, Loader2 } from "lucide-react";
import { useCallback, useState } from "react";

import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/Dialog";
import { cn, formatDate, getDocumentUrl } from "@/lib/utils";
import type { DocumentItem as DocumentActionsItem } from "@/types/retirement";

import { DocumentActions } from "./DocumentActions";

/** Document item shape used by this dialog — الحقول كاملة لتمريرها لنموذج التعديل. */
export interface DocumentItem {
    documentID: number;
    documentTypeID: number;
    documentTypeName: string;
    fileName: string;
    filePath?: string | null;
    notes?: string | null;
    uploadedByUser?: string | null;
    createdAt: string;
    entityType?: string | null;
    entityName?: string | null;
    entitledID?: number | null;
    agentID?: number | null;
    guardianID?: number | null;
    retireeID?: number | null;
    documentNumber?: string | null;
    issueDate?: string | null;
    expiryDate?: string | null;
    pendingReviewStatus?: number | null;
}

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    document: DocumentItem | null;
    fileId?: number;
    onUploaded?: () => void;
    onRefresh?: () => void;
}

function getFileType(fileName: string): "pdf" | "image" | "unknown" {
    const ext = fileName.toLowerCase().split(".").pop();
    if (ext === "pdf") return "pdf";
    if (["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"].includes(ext || "")) return "image";
    return "unknown";
}

function mapToActionsItem(doc: DocumentItem): DocumentActionsItem {
    return {
        documentID: doc.documentID,
        documentTypeID: doc.documentTypeID,
        documentTypeName: doc.documentTypeName,
        fileName: doc.fileName,
        filePath: doc.filePath ?? undefined,
        notes: doc.notes ?? undefined,
        uploadedByUser: doc.uploadedByUser ?? undefined,
        createdAt: doc.createdAt,
        entityType: doc.entityType ?? undefined,
        entityName: doc.entityName ?? undefined,
        pendingReviewStatus: doc.pendingReviewStatus ?? undefined,
        entitledID: doc.entitledID ?? undefined,
        agentID: doc.agentID ?? undefined,
        guardianID: doc.guardianID ?? undefined,
        retireeID: doc.retireeID ?? undefined,
        documentNumber: doc.documentNumber ?? undefined,
        issueDate: doc.issueDate ?? undefined,
        expiryDate: doc.expiryDate ?? undefined,
    };
}

export function DocumentViewerDialog({
    open,
    onOpenChange,
    document,
    fileId,
    onUploaded,
    onRefresh,
}: Props) {
    const [isLoading, setIsLoading] = useState(true);

    const handleActionSuccess = useCallback(() => {
        onRefresh?.();
        onUploaded?.();
    }, [onRefresh, onUploaded]);

    if (!document) return null;

    const fullUrl = document.filePath ? getDocumentUrl(document.filePath) : null;

    const fileType = document.filePath ? getFileType(document.fileName) : "unknown";

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent
                className={cn(
                    "max-w-[95vw] w-full h-[92dvh] p-0 gap-0 overflow-hidden",
                    "flex flex-col rounded-xl border shadow-2xl"
                )}
            >
                {/* ── Header ── */}
                <DialogHeader className="shrink-0 px-5 py-4 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 z-10">
                    <DialogTitle className="text-base sm:text-lg font-semibold leading-tight truncate">
                        {document.documentTypeName}
                    </DialogTitle>
                    <DialogDescription className="text-xs sm:text-sm text-muted-foreground flex items-center gap-1.5 mt-1">
                        <span>بواسطة:</span>
                        <span className="font-medium text-foreground">
                            {document.uploadedByUser || "نظام"}
                        </span>
                        <span className="text-border mx-1">|</span>
                        <span>{formatDate(document.createdAt)}</span>
                    </DialogDescription>
                </DialogHeader>

                {/* ── Toolbar (أزرار الإجراءات) ── */}
                {fileId && (
                    <div className="shrink-0 flex items-center justify-end gap-1 px-4 py-2 border-b bg-muted/40">
                        <DocumentActions
                            document={mapToActionsItem(document)}
                            fileId={fileId}
                            actions={["download", "edit", "note", "process", "delete"]}
                            size="sm"
                            onRefresh={handleActionSuccess}
                        />
                    </div>
                )}

                {/* ── Viewer ── */}
                <div className="flex-1 relative min-h-0 bg-muted/20 flex items-center justify-center p-3 sm:p-6">
                    {fullUrl ? (
                        <div className="relative w-full h-full flex items-center justify-center">
                            {/* Loading */}
                            {isLoading && (
                                <div className="absolute inset-0 z-10 flex flex-col items-center justify-center gap-3 text-muted-foreground">
                                    <Loader2 className="h-8 w-8 animate-spin" />
                                    <span className="text-sm">جاري تحميل الملف...</span>
                                </div>
                            )}

                            {fileType === "pdf" ? (
                                <iframe
                                    src={fullUrl}
                                    className={cn(
                                        "w-full h-full rounded-lg border bg-white shadow-sm",
                                        isLoading && "opacity-0"
                                    )}
                                    title={document.documentTypeName}
                                    onLoad={() => setIsLoading(false)}
                                />
                            ) : fileType === "image" ? (
                                <img
                                    src={fullUrl}
                                    alt={document.documentTypeName}
                                    className={cn(
                                        "max-w-full max-h-full object-contain rounded-lg border bg-white shadow-sm",
                                        isLoading && "opacity-0"
                                    )}
                                    onLoad={() => setIsLoading(false)}
                                    onError={() => setIsLoading(false)}
                                />
                            ) : (
                                <div className="flex flex-col items-center justify-center gap-4 text-muted-foreground">
                                    <FileWarning className="h-12 w-12 text-amber-500" />
                                    <div className="text-center space-y-1">
                                        <p className="font-medium text-foreground">
                                            نوع الملف غير مدعوم للعرض المباشر
                                        </p>
                                        <p className="text-sm">{document.fileName}</p>
                                    </div>
                                    <a
                                        href={fullUrl}
                                        download={document.fileName}
                                        className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground shadow hover:bg-primary/90 transition-colors"
                                    >
                                        تحميل الملف
                                    </a>
                                </div>
                            )}
                        </div>
                    ) : (
                        <div className="flex flex-col items-center justify-center gap-3 text-muted-foreground">
                            <FileWarning className="h-10 w-10 text-destructive" />
                            <div className="text-center">
                                <p className="font-medium text-foreground">مسار الملف غير متاح</p>
                                <p className="text-sm mt-0.5">
                                    لا يوجد ملف مرفق بهذا المستند
                                </p>
                            </div>
                        </div>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    );
}