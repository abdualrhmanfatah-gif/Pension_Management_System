<<<<<<< HEAD
import { AlertCircle, Check, ChevronDown, ChevronUp, Eye,FileEdit, Plus, X } from "lucide-react";
import { useMemo, useState } from "react";
=======
import { useMemo, useState } from "react";
import { Check, X, ChevronDown, ChevronUp, Plus, AlertCircle, FileEdit, Eye } from "lucide-react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

import { Badge } from "@/components/ui";
import { Button } from "@/components/ui";
import { Dialog, DialogContent } from "@/components/ui/Dialog";
import { cn, formatDate, getDocumentUrl } from "@/lib/utils";
<<<<<<< HEAD
import type { FieldChangeDetail,FileReviewItem } from "@/types/retirement";
=======
import type { FileReviewItem, FieldChangeDetail } from "@/types/retirement";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { REVIEW_ITEM_STATUS } from "@/types/retirement";

interface ReviewItemCardProps {
    item: FileReviewItem;
    onApprove?: (itemId: number) => void;
    onReject?: (itemId: number) => void;
}

function isLikelyFilePath(val: string | null | undefined): boolean {
    if (!val) return false;
    const v = val.trim();
    if (!v) return false;
    if (/\.\w{2,5}$/.test(v)) return true;
    if ((v.includes("/") || v.includes("\\")) && v.length > 3) return true;
    if (/^(uploads?|files?|documents?|images?|media|wwwroot)\//i.test(v)) return true;
    return false;
}

export function ReviewItemCard({ item, onApprove, onReject }: ReviewItemCardProps) {
    const [expanded, setExpanded] = useState(false);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);
    const isPending = item.status === REVIEW_ITEM_STATUS.PENDING;
    const isApproved = item.status === REVIEW_ITEM_STATUS.APPROVED;
    const isRejected = item.status === REVIEW_ITEM_STATUS.REJECTED;

    const getFullUrl = (path: string | null | undefined): string => {
        if (!path) return "";
        if (path.startsWith("http")) return path;
        return getDocumentUrl(path) ?? "";
    };

    const fieldDetails = useMemo(() => {
        if (!item.fieldChangesJson) return null;
        try {
            const parsed = JSON.parse(item.fieldChangesJson);
            return parsed.map((d: any) => {
                const isMediaRaw = d.isMedia ?? d.IsMedia ?? false;
                const newVal = d.newValue ?? d.NewValue ?? null;
                const looksLikeFile = typeof newVal === "string" && (
                    newVal.includes("uploads/") ||
                    newVal.includes("documents/") ||
                    newVal.includes(".pdf") ||
                    newVal.includes(".jpg") ||
                    newVal.includes(".png") ||
                    newVal.includes(".jpeg") ||
                    newVal.includes(".doc") ||
                    newVal.includes(".xls")
                );
                return {
                    fieldName: d.fieldName ?? d.FieldName ?? "",
                    oldValue: d.oldValue ?? d.OldValue ?? null,
                    newValue: newVal,
                    isMedia: isMediaRaw || looksLikeFile,
                };
            }) as FieldChangeDetail[];
        } catch {
            return null;
        }
    }, [item.fieldChangesJson]);

    const hasFieldDetails = fieldDetails && fieldDetails.length > 0;

    const formatValue = (val: string | null | undefined) => {
        if (!val) return "—";
        if (val.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/)) {
            return formatDate(val);
        }
        return val;
    };

    const isActualMedia = (detail: FieldChangeDetail): boolean => {
        if (!detail.isMedia) return false;
        return isLikelyFilePath(detail.newValue) || isLikelyFilePath(detail.oldValue);
    };

    const mainOldValue = formatValue(item.oldValue);
    const mainNewValue = formatValue(item.newValue);
    const hasOldValue = !!item.oldValue && item.oldValue.trim() !== "" && item.oldValue !== "جديد";
    const hasNewValue = !!item.newValue && item.newValue.trim() !== "";
    const isCreateAction = item.oldValue === "جديد";

    const isMainOldMedia = hasOldValue && isLikelyFilePath(item.oldValue);
    const isMainNewMedia = hasNewValue && isLikelyFilePath(item.newValue);

    const mediaFields = hasFieldDetails ? fieldDetails.filter(d => isActualMedia(d) && (d.newValue || d.oldValue)) : [];
    const textFields = hasFieldDetails ? fieldDetails.filter(d => !isActualMedia(d)) : [];

    return (
        <>
            <div
                dir="rtl"
                className={cn(
                    "rounded-xl border p-4 transition-all duration-200 shadow-sm w-full overflow-auto max-h-[80vh]",
                    isApproved && "border-emerald-200 bg-emerald-50/50 dark:border-emerald-800 dark:bg-emerald-950/20",
                    isRejected && "border-rose-200 bg-rose-50/50 dark:border-rose-800 dark:bg-rose-950/20",
<<<<<<< HEAD
                    isPending && "border-border bg-card hover:border-primary hover:shadow-md"
=======
                    isPending && "border-border bg-card hover:border-primary/30 hover:shadow-md"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                )}
            >
                {/* Header */}
                <div className="flex items-center justify-between mb-3 min-w-0">
                    {item.sectionName && (
                        <span className="text-xs font-medium text-muted-foreground bg-muted px-2.5 py-1 rounded-full truncate">
                            {item.sectionName}
                        </span>
                    )}
                    <Badge
                        variant={isApproved ? "success" : isRejected ? "danger" : "warning"}
                        className="text-[10px] font-semibold px-2.5 py-0.5 shrink-0"
                    >
                        <span className="flex items-center gap-1">
                            {isApproved && <Check className="w-3 h-3" />}
                            {isRejected && <X className="w-3 h-3" />}
                            {isPending && <AlertCircle className="w-3 h-3" />}
                            {isApproved ? "معتمد" : isRejected ? "مرفوض" : "بانتظار المراجعة"}
                        </span>
                    </Badge>
                </div>

                {/* Title */}
                <div className="flex items-start gap-2 mb-3">
                    <FileEdit className="w-4 h-4 text-muted-foreground mt-0.5 shrink-0" />
                    <p className="text-sm font-semibold leading-relaxed whitespace-nowrap">
                        {item.entityDisplayName && (
                            <span className="text-muted-foreground">{item.entityDisplayName} — </span>
                        )}
                        {item.fieldName ? (
                            <span className="text-foreground">{item.fieldName}</span>
                        ) : isCreateAction && fieldDetails && fieldDetails.length > 0 ? (
                            <span className="text-foreground">
                                {fieldDetails.map(d => d.fieldName).filter(Boolean).join("، ") || "حقول جديدة"}
                            </span>
                        ) : (
                            <span className="text-muted-foreground">—</span>
                        )}
                    </p>
                </div>

                {/* Change Display */}
                {isCreateAction ? (
                    <div className="bg-emerald-50 dark:bg-emerald-950/20 rounded-lg p-3 border border-emerald-100 dark:border-emerald-900">
                        <div className="flex items-center gap-2 min-w-0">
                            <div className="w-6 h-6 rounded-full bg-emerald-100 dark:bg-emerald-900 flex items-center justify-center shrink-0">
                                <Plus className="w-3.5 h-3.5 text-emerald-700 dark:text-emerald-400" />
                            </div>
                            <span className="text-sm font-bold text-emerald-700 dark:text-emerald-400 truncate">إنشاء جديد</span>
                        </div>

                        {hasFieldDetails ? (
                            <>
                                <p className="text-sm font-bold text-emerald-600 dark:text-emerald-500 mt-2 mr-8">
                                    {fieldDetails.length} حقل تم إدخاله
                                </p>

                                {textFields.length > 0 && (
                                    <div className="mt-2 mr-8 space-y-1.5">
                                        {textFields.map((d, i) => (
                                            <div key={i} className="flex items-center justify-between gap-4 text-xs bg-emerald-100/60 dark:bg-emerald-900/40 rounded-lg px-3 py-1.5 w-max min-w-full">
                                                <span className="font-bold text-emerald-800 dark:text-emerald-300 shrink-0">{d.fieldName}</span>
                                                <span className="font-bold text-emerald-600 dark:text-emerald-400 text-start whitespace-nowrap block" dir="ltr">
                                                    {formatValue(d.newValue)}
                                                </span>
                                            </div>
                                        ))}
                                    </div>
                                )}

                                {mediaFields.length > 0 && (
                                    <div className="mt-2 mr-8">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="h-auto gap-2 text-xs font-bold text-emerald-700 dark:text-emerald-400 bg-emerald-100 dark:bg-emerald-900 border-emerald-200 dark:border-emerald-800 hover:bg-emerald-200 dark:hover:bg-emerald-800 w-max min-w-full justify-start"
                                            onClick={() => {
                                                const firstMedia = mediaFields[0];
                                                if (firstMedia?.newValue) setPreviewUrl(firstMedia.newValue);
                                            }}
                                        >
                                            <Eye className="w-4 h-4 shrink-0" />
                                            <span className="text-start whitespace-nowrap block">
                                                {mediaFields.length === 1
                                                    ? `عرض المرفق (${mediaFields[0].fieldName})`
                                                    : `عرض ${mediaFields.length} مرفقات`
                                                }
                                            </span>
                                        </Button>
                                    </div>
                                )}
                            </>
                        ) : (
                            hasNewValue && (
                                <div className="mt-2 mr-8">
                                    <div className="min-w-0 overflow-x-auto">
                                        <span className="text-sm font-bold text-emerald-600 dark:text-emerald-500 text-start whitespace-nowrap block" dir="ltr">
                                            {mainNewValue}
                                        </span>
                                    </div>
                                </div>
                            )
                        )}
                    </div>
                ) : (
<<<<<<< HEAD
                    <div className="bg-muted/40 rounded-lg p-3 border border-border space-y-2 w-max min-w-full">
=======
                    <div className="bg-muted/40 rounded-lg p-3 border border-border/50 space-y-2 w-max min-w-full">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <div className="flex items-start justify-between gap-4">
                            <span className="text-xs font-bold text-muted-foreground shrink-0">القيمة السابقة:</span>
                            {isMainOldMedia ? (
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-auto gap-1.5 text-xs font-bold text-rose-600 bg-rose-100/50 dark:bg-rose-900/30 hover:bg-rose-200/50 dark:hover:bg-rose-900/50 line-through px-2 py-1"
                                    onClick={() => setPreviewUrl(item.oldValue!)}
                                >
                                    <Eye className="w-3.5 h-3.5" />
                                    <span>عرض المرفق السابق</span>
                                </Button>
                            ) : (
                                <span className={cn(
                                    "text-xs font-bold text-start whitespace-nowrap block",
                                    hasOldValue ? "text-rose-600 line-through" : "text-muted-foreground"
                                )} dir="ltr">
                                    {mainOldValue}
                                </span>
                            )}
                        </div>
                        <div className="h-px bg-border/50" />
                        <div className="flex items-start justify-between gap-4">
                            <span className="text-xs font-bold text-muted-foreground shrink-0">القيمة الجديدة:</span>
                            {isMainNewMedia ? (
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-auto gap-1.5 text-xs font-bold text-emerald-600 bg-emerald-100/50 dark:bg-emerald-900/30 hover:bg-emerald-200/50 dark:hover:bg-emerald-900/50 px-2 py-1"
                                    onClick={() => setPreviewUrl(item.newValue!)}
                                >
                                    <Eye className="w-3.5 h-3.5" />
                                    <span>عرض المرفق الجديد</span>
                                </Button>
                            ) : (
                                <span className={cn(
                                    "text-xs font-bold text-start whitespace-nowrap block",
                                    hasNewValue ? "text-emerald-600" : "text-muted-foreground"
                                )} dir="ltr">
                                    {mainNewValue}
                                </span>
                            )}
                        </div>
                    </div>
                )}

                {/* Field Details */}
                {hasFieldDetails && (
                    <div className="mt-3">
                        <Button
                            variant="ghost"
                            className="w-full justify-between text-sm font-bold text-muted-foreground hover:text-foreground bg-muted/30 px-3 py-2 h-auto"
                            onClick={() => setExpanded(!expanded)}
                        >
                            <span className="flex items-center gap-1.5 truncate">
                                {expanded ? <ChevronUp className="w-4 h-4 shrink-0" /> : <ChevronDown className="w-4 h-4 shrink-0" />}
                                <span className="truncate">تفاصيل التعديل ({fieldDetails.length} حقل)</span>
                            </span>
                        </Button>
                        {expanded && (
                            <div className="mt-2 space-y-2">
                                {fieldDetails.map((detail, idx) => {
                                    const detailHasOldValue = !!detail.oldValue && detail.oldValue.trim() !== "";
                                    const detailHasNewValue = !!detail.newValue && detail.newValue.trim() !== "";
                                    const detailIsMedia = isActualMedia(detail);
                                    const documentUrl = detailHasNewValue ? detail.newValue : detailHasOldValue ? detail.oldValue : null;
                                    return (
                                        <div key={idx} className="bg-background rounded-lg p-3 border border-border">
                                            {detailIsMedia && documentUrl ? (
                                                <div className="space-y-2">
                                                    <p className="text-sm font-bold text-foreground whitespace-nowrap">{detail.fieldName}</p>
                                                    <Button
                                                        variant="outline"
                                                        size="sm"
<<<<<<< HEAD
                                                        className="h-auto gap-2 text-xs font-bold text-primary bg-primary/10 border-primary hover:bg-primary/20 w-max min-w-full justify-start"
=======
                                                        className="h-auto gap-2 text-xs font-bold text-primary bg-primary/10 border-primary/20 hover:bg-primary/20 w-max min-w-full justify-start"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                        onClick={() => setPreviewUrl(documentUrl)}
                                                    >
                                                        <Eye className="w-4 h-4 shrink-0" />
                                                        <span className="text-start whitespace-nowrap block">عرض المرفق</span>
                                                    </Button>
                                                </div>
                                            ) : (
                                                <>
                                                    <p className="text-sm font-bold text-foreground mb-2 whitespace-nowrap">{detail.fieldName}</p>
                                                    <div className="space-y-1.5 w-max min-w-full">
                                                        <div className="flex items-start justify-between gap-4">
                                                            <span className="text-xs font-bold text-muted-foreground shrink-0">من:</span>
                                                            <span className={cn(
                                                                "text-xs font-bold text-start whitespace-nowrap block",
                                                                detailHasOldValue ? "text-rose-600" : "text-muted-foreground"
                                                            )} dir="ltr">
                                                                {detailHasOldValue ? formatValue(detail.oldValue) : "جديد"}
                                                            </span>
                                                        </div>
                                                        <div className="flex items-start justify-between gap-4">
                                                            <span className="text-xs font-bold text-muted-foreground shrink-0">إلى:</span>
                                                            <span className={cn(
                                                                "text-xs font-bold text-start whitespace-nowrap block",
                                                                detailHasNewValue ? "text-emerald-600" : "text-muted-foreground"
                                                            )} dir="ltr">
                                                                {detailHasNewValue ? formatValue(detail.newValue) : "—"}
                                                            </span>
                                                        </div>
                                                    </div>
                                                </>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>
                )}

                {/* Rejection Reason */}
                {item.reviewNotes && (
                    <div className="mt-3 p-3 bg-rose-50 dark:bg-rose-950/20 border border-rose-100 dark:border-rose-900 rounded-lg overflow-hidden">
                        <div className="flex items-start gap-2 min-w-0">
                            <AlertCircle className="w-4 h-4 text-rose-500 mt-0.5 shrink-0" />
                            <div className="min-w-0 overflow-hidden">
                                <p className="text-xs font-bold text-rose-700 mb-0.5">سبب الرفض</p>
                                <p className="text-sm font-bold text-rose-600 leading-relaxed break-words">{item.reviewNotes}</p>
                            </div>
                        </div>
                    </div>
                )}

                {/* Actions */}
                {isPending && onApprove && onReject && (
                    <div className="flex gap-2 mt-4">
                        <Button
                            variant="outline"
                            size="sm"
                            className="flex-1 h-9 text-emerald-600 border-emerald-200 hover:bg-emerald-50 hover:text-emerald-700 hover:border-emerald-300 transition-colors"
                            onClick={() => onApprove(item.itemId)}
                        >
                            <Check className="w-4 h-4 ml-1.5" />
                            اعتماد
                        </Button>
                        <Button
                            variant="outline"
                            size="sm"
                            className="flex-1 h-9 text-rose-600 border-rose-200 hover:bg-rose-50 hover:text-rose-700 hover:border-rose-300 transition-colors"
                            onClick={() => onReject(item.itemId)}
                        >
                            <X className="w-4 h-4 ml-1.5" />
                            رفض
                        </Button>
                    </div>
                )}
            </div>

            {/* Document Preview Dialog */}
            <Dialog open={!!previewUrl} onOpenChange={() => setPreviewUrl(null)}>
                <DialogContent className="max-w-4xl max-h-[90vh] p-0 overflow-hidden">
                    {previewUrl && (
                        previewUrl.endsWith(".pdf") ? (
                            <iframe
                                src={getFullUrl(previewUrl)}
                                className="w-full h-[80vh]"
                                title="معاينة الوثيقة"
                            />
                        ) : (
                            <img
                                src={getFullUrl(previewUrl)}
                                alt="معاينة الوثيقة"
                                className="w-full h-auto max-h-[80vh] object-contain"
                            />
                        )
                    )}
                </DialogContent>
            </Dialog>
        </>
    );
}