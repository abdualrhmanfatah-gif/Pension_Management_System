<<<<<<< HEAD
=======
import { useCallback, useEffect, useMemo, useState } from "react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
    AlertTriangle,
    CheckCircle,
    Eye,
    FileText,
    FileWarning,
    Info,
    ListChecks,
    Pencil,
    Plus,
    Printer,
    StickyNote,
    Trash2,
    Upload,
    User,
} from "lucide-react";
<<<<<<< HEAD
import { useCallback, useEffect, useMemo, useState } from "react";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { toast } from "sonner";

import api from "@/api/axios";
import { retirementApi } from "@/api/retirement";
<<<<<<< HEAD
import { AddMissingDocumentDialog } from "@/components/domain/retirement/AddMissingDocumentDialog";
import { BulkUploadDialog } from "@/components/domain/retirement/BulkUploadDialog";
import { DocumentViewerDialog } from "@/components/domain/retirement/DocumentViewerDialog";
import { EditDocumentDialog } from "@/components/domain/retirement/EditDocumentDialog";
import { MissingDocActionDialog } from "@/components/domain/retirement/MissingDocActionDialog";
import { UploadDocumentDialog } from "@/components/domain/retirement/UploadDocumentDialog";
import {
    Badge,
    Button,
=======
import { getDocumentUrl } from "@/lib/utils";
import {
    Badge,
    Button,
    Card,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    type CustomColumnDef,
    DataTable,
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
<<<<<<< HEAD
    LoadingIndicator,
    Text,
    TabHeader,
    useConfirm,
} from "@/components/ui";
import { PENDING_REVIEW_STATUS } from "@/constants";
import { useApiMutation, useCreateShortcut } from "@/hooks";
import { getDocumentUrl } from "@/lib/utils";
=======
    Heading,
    LoadingIndicator,
    Text,
    useConfirm,
} from "@/components/ui";
import { PENDING_REVIEW_STATUS } from "@/constants";
import { AddMissingDocumentDialog } from "@/components/domain/retirement/AddMissingDocumentDialog";
import { DocumentViewerDialog } from "@/components/domain/retirement/DocumentViewerDialog";
import { EditDocumentDialog } from "@/components/domain/retirement/EditDocumentDialog";
import { MissingDocActionDialog } from "@/components/domain/retirement/MissingDocActionDialog";
import { BulkUploadDialog } from "@/components/domain/retirement/BulkUploadDialog";
import { UploadDocumentDialog } from "@/components/domain/retirement/UploadDocumentDialog";
import { useApiMutation, useCreateShortcut } from "@/hooks";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type {
    DocumentItem,
    MissingDocumentInfo as MissingDocumentItem,
} from "@/types/retirement";

interface DocumentsTabProps {
    fileId: number;
    retireeName?: string;
}

const ENTITY_LABELS: Record<string, string> = {
    Agent: "الوكيل",
    Spouse: "الشريك",
    Guardian: "الوصي",
    EntitledPerson: "المستفيد",
    Retiree: "المتقاعد",
    Pledge: "تعهد",
    Exemption: "إعفاء",
    Suspension: "إيقاف",
    SalaryBatch: "دفعة معاشات",
};

const NOTES_TRUNCATE = 30;

function isLongNote(note: string) {
    return note.length > NOTES_TRUNCATE;
}

function truncateNote(note: string) {
    if (!isLongNote(note)) return note;
    return note.slice(0, NOTES_TRUNCATE) + "…";
}

/**
 * تبويب إدارة الوثائق والمستندات للملف التقاعدي
 */
export function DocumentsTab({ fileId, retireeName }: DocumentsTabProps) {
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();

    // ─── الصلاحيات ───
    const canUpload = useHasPermission("File.Documents.Upload");
    const canDelete = useHasPermission("File.Delete.Soft");
    const canManageMissing = useHasPermission("MissingDocument.Manage");
    const canEdit = useHasPermission("File.Documents.Update");

    // ─── حالة الحوارات ───
    const [uploadOpen, setUploadOpen] = useState(false);
    const [uploadDefaultTypeId, setUploadDefaultTypeId] = useState<number | undefined>();
    const [uploadEntityIds, setUploadEntityIds] = useState<
        | {
              retireeID?: number;
              agentID?: number;
              guardianID?: number;
              entitledID?: number;
              pledgeID?: number;
              exemptionID?: number;
          }
        | undefined
    >();
    const [addMissingOpen, setAddMissingOpen] = useState(false);
    const [actionOpen, setActionOpen] = useState(false);
    const [activeMissing, setActiveMissing] = useState<MissingDocumentItem | null>(null);
    const [bulkUploadOpen, setBulkUploadOpen] = useState(false);
    const [viewerOpen, setViewerOpen] = useState(false);
    const [viewingDoc, setViewingDoc] = useState<DocumentItem | null>(null);
    const [editOpen, setEditOpen] = useState(false);
    const [editingDoc, setEditingDoc] = useState<DocumentItem | null>(null);
    const [notesOpen, setNotesOpen] = useState(false);
    const [notesContent, setNotesContent] = useState<{ title: string; body: string } | null>(null);

    // ─── جلب البيانات ───
<<<<<<< HEAD
    // م37: مفتاح فرعي للمستودع المركب (docs/missing/types) حتى لا يتعارض مع
    // مصفوفة الوثائق في useDocumentsByFileQuery — البادئة ["documents", fileId]
    // تُبطله مع كل التحديثات القائمة على المفتاح الأب
    const { data, isLoading, error } = useQuery({
        queryKey: ["documents", fileId, "tab"],
=======
    const { data, isLoading, error } = useQuery({
        queryKey: ["documents", fileId],
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        queryFn: async () => {
            const [docs, missing, auto, types] = await Promise.all([
                retirementApi.getDocuments(fileId),
                retirementApi.getMissingDocuments(fileId),
                retirementApi.getAutoDetectedMissing(fileId).catch(() => []),
                retirementApi.getDocumentTypes(),
            ]);

            const manualMissing = missing.map((m: MissingDocumentItem) => {
                let entityName = m.entityName;
                let entityType = m.entityType;
                if (!entityName) {
                    if (m.agentName) {
                        entityName = m.agentName;
                        entityType = "Agent";
                    } else if (m.guardianName) {
                        entityName = m.guardianName;
                        entityType = "Guardian";
                    } else if (m.entitledName) {
                        entityName = m.entitledName;
                        entityType = "EntitledPerson";
                    }
                }
                return { ...m, entityName, entityType };
            });

            const autoMissing: MissingDocumentItem[] = auto.map((ad: MissingDocumentItem) => ({
                missingDocumentID: 0,
                documentTypeID: ad.documentTypeID,
                documentTypeName: ad.documentTypeName,
                status: "Pending",
                createdAt: new Date().toISOString(),
                isAutoDetected: true,
                entityName: ad.entityName,
                entityType: ad.entityType,
                retireeID: ad.retireeID || undefined,
                agentID: ad.agentID || undefined,
                agentName: ad.entityType === "Agent" ? ad.entityName : undefined,
                guardianID: ad.guardianID || undefined,
                guardianName: ad.entityType === "Guardian" ? ad.entityName : undefined,
                entitledID: ad.entitledID || undefined,
                entitledName: ad.entityType === "EntitledPerson" ? ad.entityName : undefined,
                pledgeID: ad.pledgeID || undefined,
                exemptionID: ad.exemptionID || undefined,
                notes: undefined,
            }));

            return {
                documents: docs,
                missingDocs: [...manualMissing, ...autoMissing],
                docTypes: types,
            };
        },
    });

    const documents = data?.documents ?? [];
    const missingDocs = data?.missingDocs ?? [];
    const docTypes = data?.docTypes ?? [];

    // ─── إعادة تحميل البيانات ───
    const refreshData = useCallback(() => {
        queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
        queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
    }, [queryClient, fileId]);

    // ─── اختصارات لوحة المفاتيح ───
    const handleOpenUpload = useCallback(() => {
        if (canUpload) setUploadOpen(true);
    }, [canUpload]);
    useCreateShortcut(handleOpenUpload, canUpload);

    // ─── معالجة الأخطاء ───
    useEffect(() => {
        if (error) toast.error("حدث خطأ أثناء تحميل بيانات الوثائق");
    }, [error]);

    // ─── حذف وثيقة ───
    const deleteMutation = useApiMutation(
        (id: number) => retirementApi.deleteDocument(id),
        {
            successMessage: "تم حذف الوثيقة بنجاح",
            invalidateKeys: [["documents", fileId], ["retirement-file"]],
        }
    );

    const handleDelete = useCallback(
        async (doc: DocumentItem) => {
            const ok = await confirm({
                title: "حذف الوثيقة",
                description: `هل أنت متأكد من حذف الوثيقة "${doc.documentTypeName}" نهائياً؟`,
                variant: "danger",
            });
            if (ok) deleteMutation.mutate(doc.documentID);
        },
        [confirm, deleteMutation]
    );

    // ─── طباعة جميع الوثائق ───
    const handlePrintAll = useCallback(() => {
        if (!documents.length) return;

        const printWindow = window.open("", "_blank", "width=900,height=700");
        if (!printWindow) {
            toast.error("يرجى السماح لنافذة منبثقة بالظهور للطباعة");
            return;
        }

        const docsHtml = documents
            .filter((doc: DocumentItem) => doc.filePath)
            .map((doc: DocumentItem) => {
                const filePath = doc.filePath!;
                const cleanPath = filePath.startsWith("/") ? filePath.slice(1) : filePath;
                const url = getDocumentUrl(cleanPath) ?? `/${cleanPath}`;
                const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(filePath);
                const isPdf = /\.pdf$/i.test(filePath);

                if (isImage) {
                    return `<div class="doc-page"><img src="${url}" /><p class="doc-title">${doc.documentTypeName}</p></div>`;
                }
                if (isPdf) {
                    return `<div class="doc-page"><iframe src="${url}"></iframe><p class="doc-title">${doc.documentTypeName}</p></div>`;
                }
                return `<div class="doc-page"><a href="${url}" target="_blank">${doc.documentTypeName} — افتح الملف</a></div>`;
            })
            .join("\n");

        printWindow.document.write(`<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
    <meta charset="UTF-8" />
    <title>طباعة وثائق الملف</title>
    <link href="https://fonts.googleapis.com/css2?family=Noto+Kufi+Arabic:wght@400;600;700&display=swap" rel="stylesheet">
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Noto Kufi Arabic', sans-serif; direction: rtl; padding: 24px; background: var(--bg-card); }
        .doc-page { page-break-after: always; text-align: center; margin-bottom: 24px; }
        .doc-page:last-child { page-break-after: auto; }
        .doc-page img, .doc-page iframe { max-width: 100%; max-height: 88vh; object-fit: contain; border: 1px solid #e5e7eb; border-radius: 8px; }
        .doc-page iframe { width: 100%; height: 88vh; border: none; }
        .doc-page a { display: inline-block; margin-top: 20px; padding: 10px 24px; background: #0066cc; color: white; text-decoration: none; border-radius: 6px; font-weight: 600; }
        .doc-title { margin-top: 12px; font-size: 13px; color: #6b7280; font-weight: 600; }
        h2 { font-size: 18px; font-weight: 700; margin-bottom: 24px; color: #111827; }
    </style>
</head>
<body>
    <h2>وثائق الملف التقاعدي</h2>
    ${docsHtml}
    <script>
        let loaded = 0;
        const els = document.querySelectorAll('img, iframe');
        const total = els.length;
        function checkReady() {
            loaded++;
            if (loaded >= total) setTimeout(() => window.print(), 600);
        }
        els.forEach(el => {
            if (el.tagName === 'IMG' && el.complete) checkReady();
            else el.onload = checkReady;
        });
        if (total === 0) window.print();
<<<<<<< HEAD
    </script>
=======
    <\/script>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
</body>
</html>`);
        printWindow.document.close();
    }, [documents]);

    // ─── تأشير وثيقة كمسلّمة ───
    const fulfillMutation = useApiMutation(
        async (doc: MissingDocumentItem) => {
            let missingId = doc.missingDocumentID;
            if (doc.isAutoDetected || !missingId) {
                const res = await api.post<{ data: { missingDocumentID: number } }>(
                    "/missing-documents",
                    {
                        fileID: fileId,
                        documentTypeID: doc.documentTypeID,
                        agentID: doc.agentID,
                        guardianID: doc.guardianID,
                        entitledID: doc.entitledID,
                    }
                );
                missingId = res.data.data.missingDocumentID;
            }
            return retirementApi.fulfillMissingDocument(missingId);
        },
        {
            successMessage: "تم تأشير الوثيقة كمسلّمة بنجاح",
            invalidateKeys: [["documents", fileId], ["retirement-file"]],
        }
    );

    const handleFulfill = useCallback(
        async (doc: MissingDocumentItem) => {
            const ok = await confirm({
                title: "تأشير كمسلّمة",
                description: `هل أنت متأكد من استلام وتأشير وثيقة "${doc.documentTypeName}" كمسلّمة ورقيًا دون رفعها على النظام؟`,
                variant: "default",
            });
            if (ok) fulfillMutation.mutate(doc);
        },
        [confirm, fulfillMutation]
    );

    // ─── تسمية الكيان ───
    const getEntityLabel = useCallback(
        (type?: string | null, name?: string | null) => {
            const label = ENTITY_LABELS[type || ""] || type;
            const finalName = name || retireeName;
            if (label && finalName) return `${label}: ${finalName}`;
            return finalName || label || "—";
        },
        [retireeName]
    );

    // ─── عرض الملاحظات الكاملة ───
    const openNotes = useCallback((title: string, body: string) => {
        setNotesContent({ title, body });
        setNotesOpen(true);
    }, []);

    // ─── أعمدة الجدول ───
    const columns = useMemo<CustomColumnDef<DocumentItem>[]>(
        () => [
            {
                key: "documentTypeName",
                header: "نوع الوثيقة",
                className: "sm:w-[220px] w-auto",
                render: (row) => (
                    <div className="flex items-center gap-3 min-w-0">
                        <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center text-primary shrink-0">
                            <FileText className="w-4 h-4" />
                        </div>
                        <div className="min-w-0">
                            <span className="font-semibold text-sm block truncate">
                                {row.documentTypeName}
                            </span>
                            <div className="flex items-center gap-1.5 mt-0.5">
                                {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                    <Badge variant="warning" className="text-[10px] h-5 px-1.5">
                                        قيد المراجعة
                                    </Badge>
                                )}
                                {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                    <Badge variant="destructive" className="text-[10px] h-5 px-1.5">
                                        مرفوض
                                    </Badge>
                                )}
                            </div>
                        </div>
                    </div>
                ),
            },
            {
                key: "entityName",
                header: "صاحب الوثيقة",
                className: "sm:w-[140px] w-auto",
                render: (row) => (
                    <div className="flex items-center gap-1.5 text-xs text-muted-foreground min-w-0">
                        <User className="w-3.5 h-3.5 shrink-0" />
                        <span className="truncate block">
                            {getEntityLabel(row.entityType, row.entityName)}
                        </span>
                    </div>
                ),
            },
            {
                key: "uploadedByUser",
                header: "بواسطة",
                className: "sm:w-[100px] w-auto",
                render: (row) => (
                    <span className="text-xs text-muted-foreground font-medium truncate block">
                        {row.uploadedByUser || "النظام"}
                    </span>
                ),
            },
            {
                key: "createdAt",
                header: "تاريخ الرفع",
                className: "sm:w-[110px] w-auto",
                render: (row) => (
                    <Badge variant="outline" className="text-[11px] font-mono truncate max-w-full">
                        {formatDate(row.createdAt)}
                    </Badge>
                ),
            },
            {
                key: "notes",
                header: "ملاحظات",
                className: "max-w-[100px]",
                render: (row) => {
                    if (!row.notes) {
<<<<<<< HEAD
                        return <span className="text-xs text-muted-foreground">—</span>;
=======
                        return <span className="text-xs text-muted-foreground/40">—</span>;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    }
                    const long = isLongNote(row.notes);
                    return (
                        <div className="flex items-start gap-1.5 text-xs text-muted-foreground min-w-0">
                            <StickyNote className="w-3.5 h-3.5 text-yellow-500 shrink-0 mt-0.5" />
                            <div className="min-w-0 overflow-hidden">
                                <span className="block truncate">
                                    {truncateNote(row.notes)}
                                </span>
                                {long && (
                                    <Button
                                        variant="link"
                                        size="sm"
                                        className="h-auto p-0 text-[11px] text-primary font-semibold underline-offset-2 mr-1"
                                        onClick={() => openNotes(row.documentTypeName, row.notes!)}
                                    >
                                        عرض التفاصيل
                                    </Button>
                                )}
                            </div>
                        </div>
                    );
                },
            },
            {
                key: "actions",
                header: "إجراءات",
                className: "sm:w-[90px] w-auto",
                render: (row) => (
                    <div className="flex items-center justify-center gap-0.5">
                        <Button
                            size="icon"
                            variant="ghost"
                            className="h-8 w-8 text-blue-600 hover:text-blue-700 hover:bg-blue-50"
                            onClick={() => {
                                setViewingDoc(row);
                                setViewerOpen(true);
                            }}
                            title="عرض"
                        >
                            <Eye className="w-4 h-4" />
                        </Button>
                        {canEdit && (
                            <Button
                                size="icon"
                                variant="ghost"
                                className="h-8 w-8 text-amber-600 hover:text-amber-700 hover:bg-amber-50"
                                onClick={() => {
                                    setEditingDoc(row);
                                    setEditOpen(true);
                                }}
                                title="تعديل"
                            >
                                <Pencil className="w-4 h-4" />
                            </Button>
                        )}
                        {canDelete && (
                            <Button
                                size="icon"
                                variant="ghost"
<<<<<<< HEAD
                                className="h-8 w-8 text-destructive hover:text-destructive hover:bg-destructive/10"
=======
                                className="h-8 w-8 text-destructive hover:text-destructive/90 hover:bg-destructive/10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                onClick={() => handleDelete(row)}
                                title="حذف"
                            >
                                <Trash2 className="w-4 h-4" />
                            </Button>
                        )}
                    </div>
                ),
            },
        ],
        [canDelete, canEdit, getEntityLabel, handleDelete, openNotes]
    );

    // ─── حالات التحميل والخطأ ───
    if (error) {
        return (
            <div className="py-16 text-center flex flex-col items-center gap-3 text-destructive">
                <AlertTriangle className="w-10 h-10" />
                <Text variant="body" className="font-bold">حدث خطأ أثناء تحميل بيانات الوثائق</Text>
                <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                        queryClient.invalidateQueries({ queryKey: ["documents", fileId] })
                    }
                >
                    إعادة المحاولة
                </Button>
            </div>
        );
    }

    if (isLoading) {
        return <LoadingIndicator message="جاري تحميل المستندات..." />;
    }

    return (
        <div className="space-y-4 md:space-y-6 animate-in fade-in duration-500">
            {/* ═══ Header ═══ */}
<<<<<<< HEAD
            {/* ═══ Header ═══ */}
            <TabHeader
                title="وثائق الملف المرفوعة"
                description="كافة المستندات المؤرشفة والمرفقة مع هذا الملف التقاعدي"
                icon={FileText}
            >
                {canUpload && (
                    <>
                        <Button
                            size="sm"
                            variant="outline"
                            onClick={() => setBulkUploadOpen(true)}
                            className="gap-1 rounded-xl font-bold h-8 md:h-10 px-4 md:px-6 transition-all text-xs md:text-sm shrink-0"
                        >
                            <Upload className="w-3.5 h-3.5 md:w-4 md:h-4 ms-1" />
                            <span className="hidden sm:inline">رفع جماعي</span>
                            <span className="sm:hidden">جماعي</span>
                        </Button>
                        <Button
                            size="sm"
                            onClick={() => {
                                setUploadDefaultTypeId(undefined);
                                setUploadEntityIds(undefined);
                                setUploadOpen(true);
                            }}
                            className="gap-1 rounded-xl font-bold h-8 md:h-10 px-4 md:px-6 transition-all text-xs md:text-sm shrink-0"
                        >
                            <Plus className="w-3.5 h-3.5 md:w-4 md:h-4 ms-1" />
                            <span className="hidden sm:inline">رفع وثيقة</span>
                            <span className="sm:hidden">وثيقة</span>
                        </Button>
                    </>
                )}
                {canManageMissing && (
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setAddMissingOpen(true)}
                        className="gap-1 rounded-xl font-bold h-8 md:h-10 px-4 md:px-6 transition-all text-xs md:text-sm shrink-0"
                    >
                        <ListChecks className="w-3.5 h-3.5 md:w-4 md:h-4 ms-1" />
                        <span className="hidden sm:inline">تسجيل نقص</span>
                        <span className="sm:hidden">نقص</span>
                    </Button>
                )}
                {documents.length > 0 && (
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={handlePrintAll}
                        className="h-8 md:h-10 px-2 md:px-3 shrink-0 rounded-xl"
                        title="طباعة الكل"
                    >
                        <Printer className="w-4 h-4" />
                    </Button>
                )}
            </TabHeader>

            {/* ═══ Missing Documents ═══ */}
            {missingDocs.length > 0 && (
                <div className="bg-destructive/[0.03] border border-destructive rounded-xl overflow-hidden">
                    {/* Header */}
                    <div className="bg-destructive/10 px-4 py-3 md:px-5 md:py-3.5 flex items-center justify-between border-b border-destructive">
=======
            <Card className="flex flex-col gap-3 bg-muted/30 p-4 md:p-5">
                <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <div className="flex items-center gap-2">
                        <FileText className="w-4 h-4 md:w-5 md:h-5 text-primary shrink-0" />
                        <Heading level="h3" className="font-bold text-sm md:text-base whitespace-nowrap">
                            وثائق الملف المرفوعة
                        </Heading>
                        <Text variant="caption" className="md:text-sm font-medium text-muted-foreground hidden sm:inline">
                            — كافة المستندات المؤرشفة والمرفقة مع هذا الملف التقاعدي
                        </Text>
                    </div>
                    <div className="flex items-center gap-1.5 md:gap-2">
                    {canUpload && (
                        <>
                            <Button
                                size="sm"
                                variant="outline"
                                onClick={() => setBulkUploadOpen(true)}
                                className="gap-1 h-7 px-2 text-[11px] md:h-9 md:text-sm md:px-3 shrink-0"
                            >
                                <Upload className="w-3.5 h-3.5" />
                                <span className="hidden sm:inline">رفع جماعي</span>
                                <span className="sm:hidden">جماعي</span>
                            </Button>
                            <Button
                                size="sm"
                                onClick={() => {
                                    setUploadDefaultTypeId(undefined);
                                    setUploadEntityIds(undefined);
                                    setUploadOpen(true);
                                }}
                                className="gap-1 h-7 px-2 text-[11px] md:h-9 md:text-sm md:px-3 shrink-0"
                            >
                                <Plus className="w-3.5 h-3.5" />
                                <span className="hidden sm:inline">رفع وثيقة</span>
                                <span className="sm:hidden">وثيقة</span>
                            </Button>
                        </>
                    )}
                    {canManageMissing && (
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setAddMissingOpen(true)}
                            className="gap-1 h-7 px-2 text-[11px] md:h-9 md:text-sm md:px-3 shrink-0"
                        >
                            <ListChecks className="w-3.5 h-3.5" />
                            <span className="hidden sm:inline">تسجيل نقص</span>
                            <span className="sm:hidden">نقص</span>
                        </Button>
                    )}
                    {documents.length > 0 && (
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={handlePrintAll}
                            className="h-7 px-2 md:h-9 md:px-3 shrink-0"
                            title="طباعة الكل"
                        >
                            <Printer className="w-4 h-4" />
                        </Button>
                    )}
                </div>
                </div>
            </Card>

            {/* ═══ Missing Documents ═══ */}
            {missingDocs.length > 0 && (
                <div className="bg-destructive/[0.03] border border-destructive/20 rounded-xl overflow-hidden">
                    {/* Header */}
                    <div className="bg-destructive/10 px-4 py-3 md:px-5 md:py-3.5 flex items-center justify-between border-b border-destructive/15">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <div className="flex items-center gap-2 text-destructive font-bold">
                            <FileWarning className="w-5 h-5" />
                            <span className="text-sm md:text-base">
                                وثائق ناقصة ({missingDocs.length})
                            </span>
                        </div>
                        <AlertTriangle className="w-5 h-5 text-destructive animate-pulse" />
                    </div>

                    {/* List */}
<<<<<<< HEAD
                    <div className="divide-y divide-destructive">
=======
                    <div className="divide-y divide-destructive/10">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        {missingDocs.map((md, idx) => (
                            <div
                                key={`${md.documentTypeID}-${md.entityType || idx}`}
                                className="px-4 py-3 md:px-5 md:py-4 flex flex-col md:flex-row md:items-center justify-between gap-3 hover:bg-destructive/[0.02] transition-colors"
                            >
                                <div className="flex items-start md:items-center gap-3">
                                    <div className="w-10 h-10 rounded-xl bg-destructive/10 flex items-center justify-center text-destructive shrink-0 mt-0.5 md:mt-0">
                                        <Info className="w-5 h-5" />
                                    </div>
                                    <div className="space-y-1 min-w-0">
                                        <Text variant="body" className="font-bold leading-tight">
                                            {md.documentTypeName}
                                        </Text>
                                        <div className="flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
                                            <span className="flex items-center gap-1 font-medium">
                                                <User className="w-3.5 h-3.5" />
                                                {getEntityLabel(md.entityType, md.entityName)}
                                            </span>
                                            <Badge
                                                variant="outline"
<<<<<<< HEAD
                                                className="text-[10px] h-5 px-1.5 border-destructive text-destructive bg-destructive/[0.05]"
=======
                                                className="text-[10px] h-5 px-1.5 border-destructive/20 text-destructive bg-destructive/[0.05]"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            >
                                                {md.status}
                                            </Badge>
                                        </div>
                                        {md.notes && (
                                            <Text variant="caption">
                                                {md.notes}
                                            </Text>
                                        )}
                                    </div>
                                </div>

                <div className="flex flex-nowrap items-center gap-1.5 overflow-x-auto">
                                    {canUpload && (
                                        <Button
                                            size="sm"
                                            variant="outline"
<<<<<<< HEAD
                                            className="h-8 px-3 text-xs font-semibold border-primary text-primary hover:bg-primary/5 gap-1.5 rounded-lg"
=======
                                            className="h-8 px-3 text-xs font-semibold border-primary/30 text-primary hover:bg-primary/5 gap-1.5 rounded-lg"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            onClick={() => {
                                                setUploadDefaultTypeId(md.documentTypeID);
                                                setUploadEntityIds({
                                                    retireeID: md.retireeID || undefined,
                                                    agentID: md.agentID || undefined,
                                                    guardianID: md.guardianID || undefined,
                                                    entitledID: md.entitledID || undefined,
                                                    pledgeID: md.pledgeID || undefined,
                                                    exemptionID: md.exemptionID || undefined,
                                                });
                                                setUploadOpen(true);
                                            }}
                                        >
                                            <Upload className="w-3.5 h-3.5" />
                                            رفع الوثيقة
                                        </Button>
                                    )}
                                    {canManageMissing && (
                                        <Button
                                            size="sm"
                                            variant="outline"
<<<<<<< HEAD
                                            className="h-8 px-3 text-xs font-semibold border-emerald-500 text-emerald-600 hover:bg-emerald-500/10 gap-1.5 rounded-lg"
=======
                                            className="h-8 px-3 text-xs font-semibold border-emerald-500/30 text-emerald-600 hover:bg-emerald-500/10 gap-1.5 rounded-lg"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            onClick={() => handleFulfill(md)}
                                            disabled={fulfillMutation.isPending}
                                        >
                                            <CheckCircle className="w-3.5 h-3.5" />
                                            مسلّمة
                                        </Button>
                                    )}
                                    {canManageMissing && (
                                        <Button
                                            size="sm"
                                            variant="outline"
<<<<<<< HEAD
                                            className="h-8 px-3 text-xs font-semibold border-destructive text-destructive hover:bg-destructive/5 rounded-lg"
=======
                                            className="h-8 px-3 text-xs font-semibold border-destructive/30 text-destructive hover:bg-destructive/5 rounded-lg"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            onClick={() => {
                                                setActiveMissing(md);
                                                setActionOpen(true);
                                            }}
                                        >
                                            إجراء إداري
                                        </Button>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* ═══ Documents DataTable ═══ */}
            <DataTable
                columns={columns}
                data={documents}
                loading={isLoading}
                emptyMessage="لا توجد وثائق مؤرشفة لهذا الملف"
                renderMobileCard={(row) => (
                    <div className="px-3 py-3 space-y-3">
                        {/* Title */}
                        <div className="flex items-center gap-3">
                            <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center text-primary shrink-0">
                                <FileText className="w-4 h-4" />
                            </div>
                            <div className="min-w-0 flex-1">
                                <div className="font-semibold text-sm truncate">
                                    {row.documentTypeName}
                                </div>
                                <div className="text-xs text-muted-foreground truncate flex items-center gap-1">
                                    <User className="w-3 h-3 shrink-0" />
                                    {getEntityLabel(row.entityType, row.entityName)}
                                </div>
                            </div>
                        </div>

                        {/* Meta */}
                        <div className="flex flex-wrap items-center gap-2">
                            {row.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING_REVIEW && (
                                <Badge variant="warning" className="text-[10px] h-5">
                                    قيد المراجعة
                                </Badge>
                            )}
                            {row.pendingReviewStatus === PENDING_REVIEW_STATUS.REJECTED && (
                                <Badge variant="destructive" className="text-[10px] h-5">
                                    مرفوض
                                </Badge>
                            )}
                            <span className="text-xs text-muted-foreground">
                                بواسطة {row.uploadedByUser || "النظام"}
                            </span>
                            <Badge variant="outline" className="text-[10px] font-mono">
                                {formatDate(row.createdAt)}
                            </Badge>
                        </div>

                        {/* Notes */}
                        {row.notes && (
                            <div className="text-xs text-muted-foreground bg-muted/50 p-2.5 rounded-lg">
                                <div className="flex items-start gap-1.5">
                                    <StickyNote className="w-3.5 h-3.5 text-yellow-500 shrink-0 mt-0.5" />
                                    <div className="min-w-0">
                                        <span className="block truncate">
                                            {truncateNote(row.notes)}
                                        </span>
                                        {isLongNote(row.notes) && (
                                            <Button
                                                variant="link"
                                                size="sm"
                                                className="h-auto p-0 text-[11px] text-primary font-semibold underline-offset-2 mr-1"
                                                onClick={() => openNotes(row.documentTypeName, row.notes!)}
                                            >
                                                عرض التفاصيل
                                            </Button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Actions */}
                        <div className="flex items-center justify-end gap-1 pt-1 border-t">
                            <Button
                                size="icon"
                                variant="ghost"
                                className="h-8 w-8 text-blue-600"
                                onClick={() => {
                                    setViewingDoc(row);
                                    setViewerOpen(true);
                                }}
                                title="عرض"
                            >
                                <Eye className="w-4 h-4" />
                            </Button>
                            {canEdit && (
                                <Button
                                    size="icon"
                                    variant="ghost"
                                    className="h-8 w-8 text-amber-600"
                                    onClick={() => {
                                        setEditingDoc(row);
                                        setEditOpen(true);
                                    }}
                                    title="تعديل"
                                >
                                    <Pencil className="w-4 h-4" />
                                </Button>
                            )}
                            {canDelete && (
                                <Button
                                    size="icon"
                                    variant="ghost"
                                    className="h-8 w-8 text-destructive"
                                    onClick={() => handleDelete(row)}
                                    title="حذف"
                                >
                                    <Trash2 className="w-4 h-4" />
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            />

            {/* ═══ Notes Detail Dialog ═══ */}
            <Dialog open={notesOpen} onOpenChange={setNotesOpen}>
                <DialogContent className="sm:max-w-lg max-w-[92vw] rounded-xl">
                    <DialogHeader className="pb-2 border-b">
                        <DialogTitle className="text-base font-bold flex items-center gap-2">
                            <StickyNote className="w-5 h-5 text-yellow-500" />
                            {notesContent?.title || "ملاحظات"}
                        </DialogTitle>
                    </DialogHeader>
                    <div className="py-4">
                        <Text variant="body" className="leading-relaxed text-foreground whitespace-pre-wrap break-words">
                            {notesContent?.body}
                        </Text>
                    </div>
                </DialogContent>
            </Dialog>

            {/* ═══ Modals ═══ */}
            <UploadDocumentDialog
                open={uploadOpen}
                onOpenChange={(open) => {
                    setUploadOpen(open);
                    if (!open) {
                        setUploadDefaultTypeId(undefined);
                        setUploadEntityIds(undefined);
                    }
                }}
                onSaved={refreshData}
                fileId={fileId}
                docTypes={docTypes}
                defaultTypeId={uploadDefaultTypeId}
                missingEntityIds={uploadEntityIds}
            />

            <AddMissingDocumentDialog
                open={addMissingOpen}
                onOpenChange={setAddMissingOpen}
                onSaved={refreshData}
                fileId={fileId}
                docTypes={docTypes}
                existingMissingTypeIds={missingDocs.map((md) => md.documentTypeID)}
            />

            <MissingDocActionDialog
                open={actionOpen}
                onOpenChange={setActionOpen}
                onSaved={refreshData}
                fileId={fileId}
                activeMissing={activeMissing}
            />

            <BulkUploadDialog
                open={bulkUploadOpen}
                onOpenChange={setBulkUploadOpen}
                onSaved={refreshData}
                fileId={fileId}
                docTypes={docTypes}
            />

            <DocumentViewerDialog
                open={viewerOpen}
                onOpenChange={setViewerOpen}
                document={viewingDoc}
                fileId={fileId}
                onUploaded={() => {
                    refreshData();
                    setViewerOpen(false);
                }}
                onRefresh={refreshData}
            />

            <EditDocumentDialog
                open={editOpen}
                onOpenChange={setEditOpen}
                document={editingDoc}
                fileId={fileId}
                onSaved={() => {
                    refreshData();
                    setEditOpen(false);
                }}
            />

            {ConfirmDialogElement}
        </div>
    );
<<<<<<< HEAD
}
=======
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
