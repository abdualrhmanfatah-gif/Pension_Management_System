import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Eye, FileSearch, FileText } from "lucide-react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";

import { archiveApi } from "@/api/archive";
import {
    Badge,
    Button,
    Card,
    FormField,
    NativeSelect,
    Text,
    Textarea,
} from "@/components/ui";
import { formatDate, getDocumentUrl } from "@/lib/utils";
import type { DocumentReviewItem } from "@/types/archive";
import { REVIEW_STATUS } from "@/types/archive";

import { DocumentActions } from "./DocumentActions";

const reviewSchema = z.object({
    reviewStatus: z.number().min(0, "حالة الفحص مطلوبة"),
    reviewNotes: z.string().min(10, "ملاحظات الفحص مطلوبة (10 أحرف على الأقل)"),
    rejectionReason: z.string().optional(),
}).refine(
    (data) => {
        if (data.reviewStatus === REVIEW_STATUS.REJECTED) {
            return data.rejectionReason && data.rejectionReason.length > 0;
        }
        return true;
    },
    {
        message: "سبب الرفض مطلوب عند رفض الوثيقة",
        path: ["rejectionReason"],
    }
);

type ReviewForm = z.infer<typeof reviewSchema>;

function getFileType(fileName: string): "pdf" | "image" | "unknown" {
    const ext = fileName.toLowerCase().split(".").pop();
    if (ext === "pdf") return "pdf";
    if (["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"].includes(ext || "")) return "image";
    return "unknown";
}

interface DocumentReviewPanelProps {
    document: DocumentReviewItem | null;
    onRefresh?: () => void;
    onViewDocument?: (doc: DocumentReviewItem) => void;
}

export function DocumentReviewPanel({
    document: doc,
    onRefresh,
    onViewDocument,
}: DocumentReviewPanelProps) {
    const navigate = useNavigate();
    const queryClient = useQueryClient();

    const reviewForm = useForm<ReviewForm>({
        resolver: zodResolver(reviewSchema),
        defaultValues: {
            reviewStatus: REVIEW_STATUS.PENDING,
            reviewNotes: "",
            rejectionReason: "",
        },
    });

    const reviewMutation = useMutation({
        mutationFn: (data: { documentId: number; reviewStatus: number; reviewNotes: string; rejectionReason?: string }) =>
            archiveApi.reviewDocument(data.documentId, {
                reviewStatus: data.reviewStatus,
                reviewNotes: data.reviewNotes,
                rejectionReason: data.rejectionReason,
            }),
        onSuccess: () => {
            toast.success("تم حفظ نتيجة الفحص بنجاح");
            queryClient.invalidateQueries({ queryKey: ["archive", "review-documents"] });
            queryClient.invalidateQueries({ queryKey: ["archive", "review-stats"] });
            onRefresh?.();
            reviewForm.reset();
        },
        onError: () => {
            toast.error("خطأ في حفظ نتيجة الفحص");
        },
    });

    const handleSubmitReview = (data: ReviewForm) => {
        if (!doc) return;
        reviewMutation.mutate({
            documentId: doc.documentId,
            reviewStatus: data.reviewStatus,
            reviewNotes: data.reviewNotes,
            rejectionReason: data.rejectionReason,
        });
    };

    const getReviewBadge = (status: number) => {
        switch (status) {
            case REVIEW_STATUS.APPROVED:
                return <Badge variant="success">معتمدة</Badge>;
            case REVIEW_STATUS.REJECTED:
                return <Badge variant="destructive">مرفوضة</Badge>;
            case REVIEW_STATUS.PENDING:
            default:
                return <Badge variant="warning">بانتظار الفحص</Badge>;
        }
    };

    if (!doc) {
        return (
            <Card className="flex-1 flex flex-col overflow-hidden">
                <div className="flex-1 flex items-center justify-center text-muted-foreground">
                    <div className="text-center">
                        <FileText className="w-12 h-12 mx-auto mb-3 opacity-30" />
                        <Text variant="body">اختر وثيقة من القائمة لعرض تفاصيلها</Text>
                    </div>
                </div>
            </Card>
        );
    }

    return (
        <Card className="flex-1 flex flex-col overflow-hidden">
            {/* Header */}
            <div className="p-4 border-b bg-muted/30 shrink-0">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3 min-w-0">
                        <FileText className="w-5 h-5 text-muted-foreground shrink-0" />
                        <div className="min-w-0">
                            <p className="font-medium truncate">{doc.fileName}</p>
                            <Text variant="caption">
                                {doc.documentTypeName} — {doc.uploadedBy} — {formatDate(doc.createdAt)}
                            </Text>
                        </div>
                    </div>
                    <div className="flex items-center gap-2 shrink-0">
                        {getReviewBadge(doc.reviewStatus)}
                        {onViewDocument && (
                            <Button variant="ghost" size="sm" onClick={() => onViewDocument(doc)}>
                                <Eye className="w-4 h-4 ml-1" />
                                عرض كامل
                            </Button>
                        )}
                        {doc.retirementFileId && (
                            <DocumentActions
                                document={{
                                    documentID: doc.documentId,
                                    documentTypeID: doc.documentTypeId,
                                    documentTypeName: doc.documentTypeName,
                                    fileName: doc.fileName,
                                    filePath: doc.filePath,
                                    notes: doc.notes,
                                    uploadedByUser: doc.uploadedBy,
                                    createdAt: doc.createdAt,
                                    pendingReviewStatus: doc.reviewStatus,
                                }}
                                fileId={doc.retirementFileId}
                                actions={["download", "edit", "note", "process", "delete"]}
                                size="sm"
                                onRefresh={() => {
                                    queryClient.invalidateQueries({ queryKey: ["archive", "review-documents"] });
                                    queryClient.invalidateQueries({ queryKey: ["archive", "review-stats"] });
                                    onRefresh?.();
                                }}
                            />
                        )}
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="flex-1 flex overflow-hidden min-h-0">
                {/* Preview */}
                <div className="flex-1 p-4 flex items-center justify-center bg-muted/10 min-h-0 overflow-auto">
                    {doc.filePath ? (
                        (() => {
                            const fileType = getFileType(doc.fileName);
                            const url = getDocumentUrl(doc.filePath);
                            if (fileType === "pdf") {
                                return (
                                    <iframe
                                        src={url ?? undefined}
                                        className="w-full h-full rounded-lg border bg-white shadow-sm"
                                        title={doc.fileName}
                                    />
                                );
                            }
                            if (fileType === "image") {
                                return (
                                    <img
                                        src={url ?? undefined}
                                        alt={doc.fileName}
                                        className="max-w-full max-h-full object-contain rounded-lg border bg-white shadow-sm"
                                    />
                                );
                            }
                            return (
                                <div className="text-center text-muted-foreground">
                                    <FileText className="w-10 h-10 mx-auto mb-2 opacity-30" />
                                    <Text variant="body">نوع الملف غير مدعوم للعرض</Text>
                                    <p className="text-xs mt-1">{doc.fileName}</p>
                                </div>
                            );
                        })()
                    ) : (
                        <div className="text-center text-muted-foreground">
                            <FileText className="w-10 h-10 mx-auto mb-2 opacity-30" />
                            <Text variant="body">لا يوجد ملف مرفق</Text>
                        </div>
                    )}
                </div>

                {/* Review Form Sidebar */}
                <div className="w-[320px] shrink-0 border-r p-4 flex flex-col gap-4 overflow-auto">
                    <h4 className="text-sm font-bold flex items-center gap-2">
                        <FileSearch className="w-4 h-4" />
                        فحص الوثيقة
                    </h4>

                    {/* Info */}
                    <div className="space-y-3 text-sm">
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">رقم الوثيقة:</span>
                            <span className="font-mono font-bold">{doc.documentId}</span>
                        </div>
                        {doc.documentNumber && (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">رقم المستند:</span>
                                <span className="font-mono">{doc.documentNumber}</span>
                            </div>
                        )}
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">نوع الوثيقة:</span>
                            <span>{doc.documentTypeName}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">القسم:</span>
                            <Badge variant="outline" className="text-xs">{doc.ownerTypeName}</Badge>
                        </div>
                        {doc.entityName && (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">صاحب الوثيقة:</span>
                                <span className="font-medium">{doc.entityName}</span>
                            </div>
                        )}
                        {doc.retireeName && !doc.entityName && (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">رقم الملف:</span>
                                <span>{doc.retireeName}</span>
                            </div>
                        )}
                        {doc.fileNumber && (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">رقم الملف:</span>
                                <Button
                                    variant="link"
                                    size="sm"
                                    className="p-0 h-auto text-xs"
                                    onClick={() => navigate(`/files/${doc.retirementFileId}`)}
                                >
                                    {doc.fileNumber}
                                </Button>
                            </div>
                        )}
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">رفع بواسطة:</span>
                            <span>{doc.uploadedBy}</span>
                        </div>
                        {doc.expiryDate && (
                            <div className="flex justify-between">
                                <span className="text-muted-foreground">انتهاء الصلاحية:</span>
                                <span>{formatDate(doc.expiryDate)}</span>
                            </div>
                        )}
                    </div>

                    {doc.notes && (
                        <>
                            <div className="h-px bg-border" />
                            <div>
                                <h5 className="text-xs font-bold text-muted-foreground mb-1">ملاحظات الوثيقة</h5>
                                <Text variant="body" className="bg-muted/50 p-2 rounded-lg whitespace-pre-wrap">{doc.notes}</Text>
                            </div>
                        </>
                    )}

                    {doc.reviewNotes && (
                        <>
                            <div className="h-px bg-border" />
                            <div>
                                <h5 className="text-xs font-bold text-muted-foreground mb-1">ملاحظات الفحص</h5>
                                <Text variant="body" className="bg-muted/50 p-2 rounded-lg whitespace-pre-wrap">{doc.reviewNotes}</Text>
                                {doc.reviewedBy && (
                                    <Text variant="caption" className="mt-1">
                                        بواسطة: {doc.reviewedBy}
                                        {doc.reviewedAt && ` — ${formatDate(doc.reviewedAt)}`}
                                    </Text>
                                )}
                            </div>
                        </>
                    )}

                    {doc.rejectionReason && (
                        <>
                            <div className="h-px bg-border" />
                            <div>
                                <h5 className="text-xs font-bold text-destructive mb-1">سبب الرفض</h5>
                                <Text variant="body" className="bg-destructive/5 p-2 rounded-lg text-destructive whitespace-pre-wrap">{doc.rejectionReason}</Text>
                            </div>
                        </>
                    )}

                    <div className="h-px bg-border" />

                    {/* Review Form */}
                    <FormField label="حالة الفحص *" error={reviewForm.formState.errors.reviewStatus?.message}>
                        <NativeSelect
                            value={reviewForm.watch("reviewStatus").toString()}
                            onChange={(val) => reviewForm.setValue("reviewStatus", Number(val))}
                            options={[
                                { value: "1", label: "بانتظار الفحص" },
                                { value: "2", label: "معتمدة" },
                                { value: "3", label: "مرفوضة" },
                            ]}
                        />
                    </FormField>

                    <FormField label="ملاحظات الفحص *" error={reviewForm.formState.errors.reviewNotes?.message}>
                        <Textarea
                            {...reviewForm.register("reviewNotes")}
                            placeholder="أدخل ملاحظات الفحص..."
                            rows={3}
                        />
                    </FormField>

                    {reviewForm.watch("reviewStatus") === REVIEW_STATUS.REJECTED && (
                        <FormField label="سبب الرفض *" error={reviewForm.formState.errors.rejectionReason?.message}>
                            <Textarea
                                {...reviewForm.register("rejectionReason")}
                                placeholder="أدخل سبب رفض الوثيقة..."
                                rows={2}
                            />
                        </FormField>
                    )}

                    <div className="mt-auto pt-2">
                        <Button
                            className="w-full"
                            onClick={reviewForm.handleSubmit(handleSubmitReview)}
                            disabled={reviewMutation.isPending}
                        >
                            {reviewMutation.isPending ? "جاري الحفظ..." : "حفظ نتيجة الفحص"}
                        </Button>
                    </div>
                </div>
            </div>
        </Card>
    );
}
