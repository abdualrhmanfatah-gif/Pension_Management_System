import { useQuery } from "@tanstack/react-query";
import { FileText, Pencil, Trash2 } from "lucide-react";

import { documentTypesApi } from "@/api/document-types";
<<<<<<< HEAD
import { DocumentTypeDialog } from "@/components/domain/admin/DocumentTypeDialog";
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation,useCrudState } from "@/hooks";
=======
import { Badge, Button, CrudPageLayout } from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import { useApiMutation,useCrudState } from "@/hooks";
import { queryKeys } from "@/constants/queryKeys";

import { DocumentTypeDialog } from "@/components/domain/admin/DocumentTypeDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Types ─────────────────────────────────────────────

interface DocumentTypeDto {
    documentTypeID: number;
    typeName: string;
    ownerType: number | null;
    isRequired: boolean;
    hasExpiry: boolean;
    maxFileSize: number | null;
    allowedExtensions: string | null;
    isActive: boolean;
}

const OWNER_TYPE_LABELS: Record<number, string> = {
    1: "ملف تقاعدي", 2: "متقاعد", 3: "مستحق", 4: "وكيل", 5: "ولي",
    6: "رهن مستندات", 7: "إعفاء مستندات", 8: "ملاحظة ملف", 9: "تعليق صرف",
    10: "دفعة رواتب", 11: "دفعة وزارية", 12: "قرار تسوية", 13: "دفعة تسوية",
    14: "تقرير محفوظ", 15: "مشروع استثماري", 16: "عقار استثماري", 17: "وحدة عقارية",
    18: "عقد إيجار", 19: "مستأجر", 20: "محفظة استثمارية", 21: "طلب صيانة",
    22: "أمر عمل", 23: "التمويل الاستثماري", 24: "الحوكمة الاستثمارية",
    25: "تقييم عقاري", 26: "دراسة جدوى", 27: "إهلاك عقاري", 28: "دفعة إيجار",
    30: "الميزانية التشغيلية", 31: "خطة استثمارية", 32: "معاملة استثمارية",
    33: "تقييم مخاطر", 40: "موظف", 41: "مراسلة",
    50: "طلب خدمة", 51: "رسالة تذكرة", 52: "حساب مواطن",
};

// ─── Page ──────────────────────────────────────────────

export function DocumentTypesPage() {
    const crud = useCrudState<DocumentTypeDto>();

    const { data: types = [], isLoading: loading, isError, refetch } = useQuery({
        queryKey: queryKeys.documents.types,
        queryFn: () => documentTypesApi.getAll(),
        retry: 0
    });

    const deleteMutation = useApiMutation(
        (id: number) => documentTypesApi.delete(id),
        {
            invalidateKeys: [queryKeys.documents.types, ["lookups"]],
            successMessage: "تم حذف نوع الوثيقة بنجاح",
            onSuccessCallback: () => crud.closeDelete(),
        }
    );

    const handleDelete = async () => {
        if (!crud.deleteTarget) return;
        deleteMutation.mutate(crud.deleteTarget.documentTypeID);
    };

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    // ─── Columns ───────────────────────────────────────

    const columns = [
        {
            key: "typeName",
            header: "نوع الوثيقة",
            render: (row: DocumentTypeDto) => (
                <div className="flex items-center gap-3">
                    <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10">
                        <FileText className="w-4 h-4 text-primary" />
                    </div>
                    <div>
                        <p className="font-medium text-foreground">{row.typeName}</p>
                        {row.isRequired && (
                            <Badge variant="danger" className="mt-1 text-[10px] sm:text-xs px-1.5 py-0 h-5 min-w-[fit-content]">
                                إلزامي
                            </Badge>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: "rules",
            header: "القواعد",
            render: (row: DocumentTypeDto) => (
                <div className="text-sm space-y-1">
                    {row.hasExpiry && (
                        <div className="flex items-center gap-1.5 text-muted-foreground">
                            <span className="w-1.5 h-1.5 rounded-full bg-yellow-500" />
                            <span>تاريخ انتهاء مطلوب</span>
                        </div>
                    )}
                    {row.maxFileSize && (
                        <div className="flex items-center gap-1.5 text-muted-foreground">
                            <span className="w-1.5 h-1.5 rounded-full bg-blue-500" />
                            <span>أقصى حجم: {row.maxFileSize} MB</span>
                        </div>
                    )}
                </div>
            ),
        },
        {
            key: "extensions",
            header: "الامتدادات",
            render: (row: DocumentTypeDto) => (
                <div className="text-xs font-mono text-muted-foreground" dir="ltr">
                    {row.allowedExtensions || "*"}
                </div>
            ),
        },
        {
            key: "ownerType",
            header: "الجهة",
            render: (row: DocumentTypeDto) => (
                <span className="text-sm text-muted-foreground">
                    {row.ownerType ? (OWNER_TYPE_LABELS[row.ownerType] || `رقم ${row.ownerType}`) : "عام"}
                </span>
            ),
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: DocumentTypeDto) => (
                <Badge variant={row.isActive ? "success" : "outline"}>
                    {row.isActive ? "نشط" : "معطل"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "الإجراءات",
            render: (row: DocumentTypeDto) => (
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={(e) => {
                            e.stopPropagation();
                            crud.openEdit(row);
                        }}
                        title="تعديل"
                    >
                        <Pencil className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={(e) => {
                            e.stopPropagation();
                            crud.openDelete(row);
                        }}
                        title="حذف"
                    >
                        <Trash2 className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    if (isError) {
        return <div className="p-6 sm:p-12 text-center text-destructive">تعذر تحميل أنواع الوثائق</div>;
    }

    return (
        <CrudPageLayout
            title="أنواع الوثائق"
            description="إعدادات أنواع المستندات وقواعد التحقق الخاصة بها"
            createLabel="إضافة نوع"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={types}
            loading={loading}
            rowKey={(row) => row.documentTypeID}
            emptyMessage="لا يوجد أنواع وثائق معرفة"
        >
            <DocumentTypeDialog
                open={crud.isFormOpen}
                onOpenChange={(o) => !o && crud.closeForm()}
                onSaved={handleSaved}
                docType={crud.selectedItem}
            />

            <ConfirmDialog
                open={crud.isDeleteOpen}
                onOpenChange={(o) => !o && crud.closeDelete()}
                title="تأكيد الحذف"
                description={`هل أنت متأكد من حذف نوع الوثيقة "${crud.deleteTarget?.typeName}"؟`}
                onConfirm={handleDelete}
                variant="danger"
            />
        </CrudPageLayout>
    );
}
