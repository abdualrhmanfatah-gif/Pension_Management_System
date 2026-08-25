import { useQuery } from "@tanstack/react-query";
import { Download, Eye, FileSearch, FileText, Plus, Trash2 } from "lucide-react";
import { useState } from "react";

import api from "@/api/axios";
import { documentsApi } from "@/api/documents";
import { Card, DataTable, EmptyState, useConfirm } from "@/components/ui";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { Button } from "@/components/ui/Button";
import { UploadDocumentDialog } from "@/components/domain/retirement/UploadDocumentDialog";
import { useApiMutation } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";
import { getDocumentUrl as getDocumentUrlUtil } from "@/lib/utils";

interface DocumentItem {
    documentID: number;
    documentTypeName: string;
    fileName: string;
    filePath?: string | null;
    fileExtension?: string | null;
    fileSize?: number | null;
    notes?: string | null;
    uploadedByUser?: string | null;
    createdAt: string;
    expiryDate?: string | null;
    issueDate?: string | null;
    isExpired?: boolean;
    hasExpiry?: boolean;
    isRequired?: boolean;
    verified?: boolean;
    documentNumber?: string | null;
    investmentPropertyID?: number | null;
    propertyUnitID?: number | null;
    propertyUnitNumber?: string | null;
}

interface UploadValues {
    file: File;
    documentTypeID: number;
    notes?: string;
    documentNumber?: string;
    issueDate?: string;
    expiryDate?: string;
}

interface Props {
    entityType: "investment-portfolio" | "investment-project" | "investment-property" | "property-unit" | "lease-contract" | "tenant" | "maintenance-ticket" | "work-order" | "property-valuation" | "depreciation-record" | "lease-payment" | "operational-expense" | "investment-plan" | "investment-txn" | "risk-assessment";
    entityId: number;
    onRefresh?: () => void;
    entityName?: string;
}

export function InvestmentDocumentsManager({
    entityType,
    entityId,
    onRefresh,
    entityName,
}: Props) {

    const { confirm, ConfirmDialogElement } = useConfirm();

    const canUpload = useHasPermission("File.Documents.Upload");
    const canDelete = useHasPermission("File.Delete.Soft");

    // تحويل نوع الكيان إلى OwnerType (DocumentOwnerType enum)
    const ownerType = (() => {
        switch (entityType) {
            case "investment-portfolio": return 20;
            case "investment-project": return 15;
            case "investment-property": return 16;
            case "property-unit": return 17;
            case "lease-contract": return 18;
            case "tenant": return 19;
            case "maintenance-ticket": return 21;
            case "work-order": return 22;
            case "property-valuation": return 25;
            case "depreciation-record": return 27;
            case "lease-payment": return 28;
            case "operational-expense": return 30;
            case "investment-plan": return 31;
            case "investment-txn": return 32;
            case "risk-assessment": return 33;
            default: return 0;
        }
    })();

    const [uploadDialogOpen, setUploadDialogOpen] = useState(false);

    const [viewerOpen, setViewerOpen] = useState(false);
    const [viewingDocument, setViewingDocument] = useState<DocumentItem | null>(null);

    const { data: documents = [], isLoading: loading } = useQuery({
        queryKey: ["documents", entityType, entityId],
        queryFn: async () => {
            const docsRes = await api.get<{ data: DocumentItem[] }>(`/documents/by-${entityType}/${entityId}`);
            return docsRes.data.data || [];
        },
        enabled: !!entityId
    });

    const getEntityIdFieldName = () => {
        switch (entityType) {
            case "investment-portfolio": return "InvestmentPortfolioID";
            case "investment-project": return "InvestmentProjectID";
            case "investment-property": return "InvestmentPropertyID";
            case "property-unit": return "PropertyUnitID";
            case "lease-contract": return "LeaseContractID";
            case "tenant": return "TenantID";
            case "maintenance-ticket": return "MaintenanceRequestID";
            case "work-order": return "WorkOrderID";
            case "property-valuation": return "AssetValuationID";
            case "depreciation-record": return "DepreciationRecordID";
            case "lease-payment": return "PaymentScheduleID";
            case "operational-expense": return "OperationalExpenseID";
            case "investment-plan": return "InvestmentPlanID";
            case "investment-txn": return "InvestmentTxnID";
            case "risk-assessment": return "RiskAssessmentID";
            default: return "";
        }
    };

    // ─── Upload Mutation ─────────────────────────────
    const uploadMutation = useApiMutation(
        async (values: UploadValues) => {
            const fieldName = getEntityIdFieldName();
            const form = new FormData();

            // الملف الأساسي
            form.append("File", values.file, values.file.name);

            // ربط الوثيقة بالكيان (نرسل بكلتا الصيغتين لضمان الربط)
            form.append(fieldName, String(entityId));

            // احتياط إضافي للتقييمات العقارية
            if (entityType === "property-valuation") {
                form.append("AssetValuationID", String(entityId));
                form.append("PropertyValuationID", String(entityId));
            }

      
            const camelFieldName = fieldName.charAt(0).toLowerCase() + fieldName.slice(1);
            if (camelFieldName !== fieldName) {
                form.append(camelFieldName, String(entityId));
            }

            form.append("DocumentTypeID", String(values.documentTypeID));

            if (values.notes) form.append("Notes", values.notes);
            if (values.documentNumber) form.append("DocumentNumber", values.documentNumber);
            if (values.issueDate) form.append("IssueDate", values.issueDate);
            if (values.expiryDate) form.append("ExpiryDate", values.expiryDate);

            return documentsApi.upload(form);
        },
        {
            successMessage: "تم رفع الوثيقة بنجاح",
            invalidateKeys: [["documents", entityType, entityId]],
            onSuccess: () => {
                setUploadDialogOpen(false);
                onRefresh?.();
            }
        }
    );

    // ─── Delete Mutation ─────────────────────────────
    const deleteMutation = useApiMutation(
        (docId: number) => documentsApi.delete(docId),
        {
            successMessage: "تم حذف الوثيقة بنجاح",
            invalidateKeys: [["documents", entityType, entityId]],
            onSuccess: () => onRefresh?.()
        }
    );

    const handleDelete = async (docId: number) => {
        if (await confirm({
            title: "حذف المستند",
            description: "هل أنت متأكد من حذف هذه الوثيقة نهائياً؟",
            variant: "danger"
        })) {
            deleteMutation.mutate(docId);
        }
    };

    const openDocumentViewer = (doc: DocumentItem) => {
        setViewingDocument(doc);
        setViewerOpen(true);
    };

    const getDocumentUrl = (path?: string | null) => {
        if (!path) return null;
        return path.startsWith("http") ? path : getDocumentUrlUtil(path) ?? `/${path}`;
    };

    const columns = [
        {
            key: "source",
            header: "المصدر",
            render: (row: DocumentItem) => {
                if (entityType === "investment-property") {
                    if (row.propertyUnitID) {
                        return (
                            <div className="flex flex-col gap-0.5">
                                <span className="inline-flex items-center w-fit px-2 py-0.5 rounded text-[10px] sm:text-xs font-medium bg-amber-50 text-amber-700 border border-amber-100">وحدة عقارية</span>
                                {row.propertyUnitNumber && <span className="text-[10px] sm:text-xs text-muted-foreground font-mono me-1">رقم: {row.propertyUnitNumber}</span>}
                            </div>
                        );
                    }
                    return <span className="inline-flex items-center px-2 py-0.5 rounded text-[10px] sm:text-xs font-medium bg-blue-50 text-blue-700 border border-blue-100 w-fit">العقار</span>;
                }
                return null;
            },
            hidden: entityType !== "investment-property"
        },
        {
            key: "documentTypeName",
            header: "نوع الوثيقة",
            render: (row: DocumentItem) => (
                <div className="flex items-center gap-2">
                    <div className="p-1.5 bg-primary/10 rounded text-primary">
                        <FileText className="w-4 h-4" />
                    </div>
                    <div className="flex flex-col">
                        <span className="font-medium">{row.documentTypeName}</span>
                        {row.isRequired && (
                            <span className="text-[10px] sm:text-xs text-amber-600 font-medium">إلزامي ✱</span>
                        )}
                    </div>
                </div>
            )
        },
        {
            key: "fileName",
            header: "اسم الملف",
            render: (row: DocumentItem) => (
                <div className="flex flex-col">
                    <span className="text-sm font-medium" dir="ltr">{row.fileName}</span>
                    {row.fileSize && (
                        <span className="text-[10px] sm:text-xs text-muted-foreground">
                            {(row.fileSize / 1024).toFixed(1)} KB
                        </span>
                    )}
                </div>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: DocumentItem) => {
                if (row.isExpired) {
                    return <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">🔴 منتهية</span>;
                }
                if (row.verified) {
                    return <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-700">🟢 صالحة</span>;
                }
                return <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-700">📄 مرفوعة</span>;
            }
        },
        {
            key: "expiryDate",
            header: "📅 تاريخ الانتهاء",
            render: (row: DocumentItem) => {
                if (!row.hasExpiry || !row.expiryDate) return <span className="text-muted-foreground text-xs">—</span>;
                const date = new Date(row.expiryDate);
                const isExpired = date < new Date();
                return (
                    <span className={`text-sm ${isExpired ? 'text-red-600 font-medium' : 'text-foreground'}`}>
                        {date.toLocaleDateString("ar-YE")}
                    </span>
                );
            }
        },
        {
            key: "createdAt",
            header: "تاريخ الرفع",
            render: (row: DocumentItem) => <span className="text-sm">{new Date(row.createdAt).toLocaleDateString("ar-YE")}</span>
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: DocumentItem) => (
                <div className="flex items-center gap-1">
                    <Button variant="ghost" size="icon" onClick={() => openDocumentViewer(row)} title="عرض">
                        <Eye className="w-4 h-4 text-primary" />
                    </Button>
                    {row.filePath && (
                        <a
                            href={getDocumentUrl(row.filePath)!}
                            download={row.fileName}
                            className="inline-flex items-center justify-center rounded-md min-w-[44px] min-h-[44px] hover:bg-muted transition-colors"
                            title="تنزيل"
                        >
                            <Download className="w-4 h-4 text-muted-foreground" />
                        </a>
                    )}
                    {canDelete && (
                        <Button variant="ghost" size="icon" onClick={() => handleDelete(row.documentID)} title="حذف">
                            <Trash2 className="w-4 h-4 text-red-500" />
                        </Button>
                    )}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-4" dir="rtl">
            <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold flex items-center gap-2">
                    <FileText className="w-5 h-5 text-primary" />
                    المستندات والوثائق
                </h3>
                {canUpload && documents.length > 0 && (
                    <Button onClick={() => setUploadDialogOpen(true)} size="sm" className="gap-2 ">
                        <Plus className="w-4 h-4" /> إضافة وثيقة
                    </Button>
                )}
            </div>

            {!loading && documents.length === 0 ? (
                <EmptyState
                    title="لا توجد وثائق مرفقة"
                    description="لم يتم رفع أي مستندات أو وثائق ثبوتية لهذا السجل حتى الآن."
                    icon={FileSearch}
                    action={canUpload ? {
                        label: "إرفاق أول وثيقة",
                        onClick: () => setUploadDialogOpen(true),
                        icon: Plus
                    } : undefined}
                />
            ) : (
                <Card className="overflow-hidden ">
                    <DataTable
                        columns={columns}
                        data={documents}
                        loading={loading}
                    />
                </Card>
            )}

            {/* استخدام المكون المطور لرفع الوثائق */}
            <UploadDocumentDialog
                open={uploadDialogOpen}
                onOpenChange={setUploadDialogOpen}
                onSubmit={async (values) => { await uploadMutation.mutateAsync(values); }}
                isLoading={uploadMutation.isPending}
                ownerType={ownerType}
                entityName={entityName}
                portal={true}
            />

            {/* Viewer Dialog */}
            <BaseFormDialog
                open={viewerOpen}
                onOpenChange={setViewerOpen}
                title={viewingDocument?.documentTypeName || "عرض الوثيقة"}
                size="4xl"
                className="max-h-[90dvh] min-h-[300px]"
                footer={
                    <div className="flex w-full items-center justify-between gap-2 flex-wrap">
                        <Button variant="outline" onClick={() => setViewerOpen(false)}>إغلاق</Button>
                        {viewingDocument?.filePath && (
                            <a
                                href={getDocumentUrl(viewingDocument.filePath) || "#"}
                                target="_blank"
                                rel="noreferrer"
                                className="inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 transition-colors gap-2"
                                download={viewingDocument.fileName}
                            >
                                <Download className="w-4 h-4" /> تنزيل الأصلي
                            </a>
                        )}
                    </div>
                }
            >
                <div className="flex-1 overflow-hidden bg-black/5 flex items-center justify-center p-4 min-h-[250px] sm:min-h-[400px] lg:min-h-[500px]">
                    {viewingDocument?.filePath ? (
                        viewingDocument.filePath.toLowerCase().endsWith('.pdf') ? (
                            <iframe
                                src={getDocumentUrl(viewingDocument.filePath)!}
                                className="w-full h-full rounded bg-white  min-h-[250px] sm:min-h-[400px] lg:min-h-[500px]"
                                title="Document Viewer"
                            />
                        ) : (
                            <img
                                src={getDocumentUrl(viewingDocument.filePath)!}
                                alt="Document"
                                className="max-w-full max-h-full object-contain rounded  bg-white"
                            />
                        )
                    ) : (
                        <div className="text-muted-foreground italic">الملف غير متوفر</div>
                    )}
                </div>
            </BaseFormDialog>

            {ConfirmDialogElement}
        </div>
    );
}
