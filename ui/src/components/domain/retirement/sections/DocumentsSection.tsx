import { useState, useCallback } from "react";
import { FileText, Upload, FileWarning, X, Loader2 } from "lucide-react";

import { Heading } from "@/components/ui";
import { Button } from "@/components/ui";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui";
import { EmptyState } from "@/components/ui";
import { useQueryClient } from "@tanstack/react-query";
import { UploadDocumentDialog } from "../UploadDocumentDialog";
import { DocumentActions } from "../DocumentActions";
import { useHasPermission } from "@/stores/authStore";
import { cn } from "@/lib/utils";
import { useDocumentsByFileQuery } from "@/hooks/queries/useDocumentsByFileQuery";
import { useMissingDocumentsQuery } from "@/hooks/queries/useMissingDocumentsQuery";
import { useDocumentBlob } from "@/hooks/useDocumentBlob";
import type { DocumentItem } from "@/types/retirement";

interface DocumentsSectionProps {
    fileId: number;
}

function DocumentPreview({ doc }: { doc: DocumentItem }) {
    const { blobUrl, loading } = useDocumentBlob(doc.filePath);
    const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(doc.filePath || "");
    const isPdf = /\.pdf$/i.test(doc.filePath || "");

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center h-full gap-3">
                <Loader2 className="w-8 h-8 text-primary animate-spin" />
                <p className="text-sm text-muted-foreground">جاري تحميل الوثيقة...</p>
            </div>
        );
    }

    if (blobUrl && isImage) {
        return (
            <div className="flex items-center justify-center h-full p-2">
                <img
                    src={blobUrl}
                    alt={doc.documentTypeName}
                    className="max-w-full max-h-full object-contain"
                />
            </div>
        );
    }

    if (blobUrl && isPdf) {
        return (
            <iframe
                src={blobUrl}
                className="w-full h-full border-0"
                title={doc.documentTypeName}
            />
        );
    }

    if (blobUrl) {
        return (
            <iframe
                src={blobUrl}
                className="w-full h-full border-0"
                title={doc.documentTypeName}
            />
        );
    }

    return (
        <div className="flex flex-col items-center justify-center h-full gap-4 p-6">
            <FileText className="w-16 h-16 text-muted-foreground/40" />
            <div className="text-center">
                <p className="font-medium">{doc.documentTypeName}</p>
                <p className="text-sm text-muted-foreground mt-1">
                    {doc.entityName && `${doc.entityName} — `}
                    {new Date(doc.createdAt).toLocaleDateString("ar-YE")}
                </p>
            </div>
        </div>
    );
}

export function DocumentsSection({ fileId }: DocumentsSectionProps) {
    const [docFilter, setDocFilter] = useState("all");
    const [selectedDoc, setSelectedDoc] = useState<DocumentItem | null>(null);
    const [uploadOpen, setUploadOpen] = useState(false);
    const [uploadDocTypeId, setUploadDocTypeId] = useState<number | undefined>(undefined);
    const queryClient = useQueryClient();

    const canUpload = useHasPermission("File.Documents.Upload");

    const { data: documents = [] } = useDocumentsByFileQuery(fileId);
    const { data: missingDocs = [] } = useMissingDocumentsQuery(fileId);

    const handleSelectDocument = useCallback((doc: DocumentItem) => {
        setSelectedDoc((prev) => prev?.documentID === doc.documentID ? null : doc);
    }, []);

    const handleUploadMissing = useCallback((docTypeId?: number) => {
        setUploadDocTypeId(docTypeId);
        setUploadOpen(true);
    }, []);

    const handleRefresh = useCallback(() => {
        queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
        queryClient.invalidateQueries({ queryKey: ["missing-documents", fileId] });
    }, [queryClient, fileId]);

    const missingCount = missingDocs.filter((d) => d.status === "Pending").length;

    return (
        <div className="flex gap-4 fade-in" style={{ height: "calc(100dvh - 200px)" }}>
            {/* ─── قائمة الوثائق (النصف الأيسر) ─── */}
            <div className={cn("flex flex-col min-w-0 h-full", selectedDoc ? "w-1/2" : "w-full")}>
                <div className="flex items-center justify-between mb-4 shrink-0">
                    <Heading level="h2">وثائق الملف</Heading>
                    {canUpload && (
                        <Button size="sm" onClick={() => handleUploadMissing()}>
                            <Upload className="w-4 h-4 ml-1" />
                            رفع وثيقة
                        </Button>
                    )}
                </div>

                <Tabs value={docFilter} onValueChange={setDocFilter} className="mb-4 shrink-0">
                    <TabsList>
                        <TabsTrigger value="all">الكل ({documents.length})</TabsTrigger>
                        <TabsTrigger value="uploaded">مرفوعة ({documents.length})</TabsTrigger>
                        <TabsTrigger value="missing">ناقصة ({missingCount})</TabsTrigger>
                    </TabsList>
                </Tabs>

                <div className="flex-1 overflow-y-auto space-y-2 min-h-0">
                    {(docFilter === "all" || docFilter === "missing") && missingCount > 0 && (
                        <div className="space-y-2 mb-4">
                            <p className="text-sm font-bold text-red-600 flex items-center gap-1">
                                <FileWarning className="w-4 h-4" />
                                وثائق ناقصة ({missingCount})
                            </p>
                            {missingDocs
                                .filter((d) => d.status === "Pending")
                                .map((doc) => (
                                    <div
                                        key={doc.missingDocumentID}
                                        className="flex items-center gap-3 p-3 rounded-lg border border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-950/20"
                                    >
                                        <FileWarning className="w-5 h-5 text-red-500 shrink-0" />
                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-medium">{doc.documentTypeName}</p>
                                            <p className="text-xs text-red-600">
                                                {doc.entityName ? `${doc.entityName} — ` : ""}مطلوبة
                                            </p>
                                        </div>
                                        <div className="flex gap-1">
                                            {canUpload && (
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-7 w-7"
                                                    onClick={() => handleUploadMissing(doc.documentTypeID)}
                                                    title="رفع وثيقة"
                                                >
                                                    <Upload className="w-3.5 h-3.5" />
                                                </Button>
                                            )}
                                        </div>
                                    </div>
                                ))}
                        </div>
                    )}

                    {(docFilter === "all" || docFilter === "uploaded") && (
                        <div className="space-y-2">
                            <p className="text-sm font-bold text-green-600">
                                وثائق مرفوعة ({documents.length})
                            </p>
                            {documents.length === 0 ? (
                                <EmptyState icon={FileText} title="لا توجد وثائق مرفوعة" />
                            ) : (
                                <div className="space-y-2">
                                    {documents.map((doc) => (
                                        <div
                                            key={doc.documentID}
                                            onClick={() => handleSelectDocument(doc)}
                                            className={cn(
                                                "flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors",
                                                selectedDoc?.documentID === doc.documentID
                                                    ? "border-primary bg-primary/5"
                                                    : "border-border bg-card hover:border-muted-foreground/30"
                                            )}
                                        >
                                            <FileText className={cn(
                                                "w-5 h-5 shrink-0",
                                                selectedDoc?.documentID === doc.documentID ? "text-primary" : "text-green-600"
                                            )} />
                                            <div className="flex-1 min-w-0">
                                                <p className="text-sm font-medium truncate">{doc.documentTypeName}</p>
                                                <p className="text-xs text-muted-foreground">
                                                    {doc.entityName && `${doc.entityName} — `}
                                                    {doc.uploadedByUser && `بواسطة: ${doc.uploadedByUser} — `}
                                                    {new Date(doc.createdAt).toLocaleDateString("ar-YE")}
                                                </p>
                                            </div>
                                            <div onClick={(e) => e.stopPropagation()}>
                                                <DocumentActions
                                                    document={doc}
                                                    fileId={fileId}
                                                    actions={["edit", "note", "delete"]}
                                                    size="sm"
                                                    onRefresh={handleRefresh}
                                                />
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </div>

            {/* ─── عارض الوثيقة (النصف الأيمن) ─── */}
            {selectedDoc && (
                <div className="w-1/2 flex flex-col border rounded-xl bg-card overflow-hidden h-full">
                    <div className="flex items-center justify-between px-4 py-3 border-b bg-muted/30 shrink-0">
                        <div className="flex items-center gap-2 min-w-0">
                            <FileText className="w-5 h-5 text-primary shrink-0" />
                            <span className="font-medium truncate">{selectedDoc.documentTypeName}</span>
                        </div>
                        <div className="flex items-center gap-1">
                            <DocumentActions
                                document={selectedDoc}
                                fileId={fileId}
                                actions={["download", "edit", "note", "process"]}
                                onRefresh={handleRefresh}
                            />
                            <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 shrink-0"
                                onClick={() => setSelectedDoc(null)}
                            >
                                <X className="w-4 h-4" />
                            </Button>
                        </div>
                    </div>
                    <div className="flex-1 overflow-hidden">
                        {selectedDoc.filePath ? (
                            <DocumentPreview doc={selectedDoc} />
                        ) : (
                            <div className="flex items-center justify-center h-full">
                                <EmptyState icon={FileText} title="لا يمكن عرض الوثيقة" />
                            </div>
                        )}
                    </div>
                </div>
            )}

            <UploadDocumentDialog
                open={uploadOpen}
                onOpenChange={setUploadOpen}
                fileId={fileId}
                defaultTypeId={uploadDocTypeId}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["missing-documents", fileId] });
                    setUploadOpen(false);
                }}
            />
        </div>
    );
}
