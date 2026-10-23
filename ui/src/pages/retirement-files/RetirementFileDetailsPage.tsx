import { useQuery,useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, ArrowRight, ClipboardCheck, FileCheck, FileText, MoreHorizontal, Printer, Shield, Trash2,User } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";

import { retirementApi } from "@/api/retirement";
import { PermissionGate } from "@/components/auth/PermissionGate";
import { CorrectLifeStatusDialog, type CorrectLifeStatusValues } from "@/components/domain/retirement/CorrectLifeStatusDialog";
import { FileLifecycleActions } from "@/components/domain/retirement/FileLifecycleActions";
import { RecordDeathDialog, type RecordDeathFormValues } from "@/components/domain/retirement/RecordDeathDialog";
import { RetirementFileDialog } from "@/components/domain/retirement/RetirementFileDialog";
import { RetirementFileSidebar } from "@/components/domain/retirement/RetirementFileSidebar";
import { StatusUpdateDialog } from "@/components/domain/retirement/StatusUpdateDialog";
import { DetailLayout } from "@/components/layout/DetailLayout";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
    AlertBanner,
    Badge,
    BaseSheet,
    Button,
    Heading,
    Skeleton,
    Text,
    useConfirm
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { FILE_DETAIL_KEY } from "@/hooks/queries";
import { useFileDetailsQuery } from "@/hooks/queries";
import { cn, getStatusColorClass } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

import { FileWorkflowManager, type TabKey } from "./FileWorkflowManager";

/**
 * صفحة تفاصيل ملف التقاعد (الملف العسكري)
 */
export function RetirementFileDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const fileId = parseInt(id || "");
    const queryClient = useQueryClient();

    const canViewStatement = useHasPermission("Retiree.View");
    const canViewHistory = useHasPermission("File.View.History");
    const canApprove = useHasPermission("File.ChangeStatus.Approve");
    const canEditFinancial = useHasPermission("File.Edit.FinancialData");
    const canEditBasic = useHasPermission("File.Edit.BasicData");
    const canUploadDocs = useHasPermission("File.Documents.Upload");
    const canReviewView = useHasPermission("File.Review.View");
    const hasAnyEntityEdit = useHasPermission("EntitledPerson.Edit", "Agent.Manage", "Guardian.Edit", "FileNote.Manage", "SalaryRecipient.Edit");

    const [searchParams, setSearchParams] = useSearchParams();
    const tabFromUrl = searchParams.get("tab") as TabKey | null;
    const validTabs: TabKey[] = ["entitled","agents","guardians","notes","suspensions","pledges","recipients","documents","statusTransitions","settlements","paymentBooks","entitlements","salaryPayments","review"];
    const [activeTab, setActiveTab] = useState<TabKey>(
        tabFromUrl && validTabs.includes(tabFromUrl) ? tabFromUrl : "entitled"
    );
    const [updateStatusOpen, setUpdateStatusOpen] = useState(false);
    const [recordDeathOpen, setRecordDeathOpen] = useState(false);
    const [correctLifeStatusOpen, setCorrectLifeStatusOpen] = useState(false);
    const [editFileOpen, setEditFileOpen] = useState(false);
    const [actionsOpen, setActionsOpen] = useState(false);

    // استعادة موضع التمرير من التخزين المؤقت
    useEffect(() => {
        const scrollKey = `scroll_pos_file_${fileId}`;
        const saved = sessionStorage.getItem(scrollKey);
        if (saved) {
            const pos = parseInt(saved, 10);
            requestAnimationFrame(() => window.scrollTo(0, pos));
        }
        const handleBeforeUnload = () => {
            sessionStorage.setItem(scrollKey, String(window.scrollY));
        };
        window.addEventListener("beforeunload", handleBeforeUnload);
        // حفظ الموضع أيضاً عند تغيير التبويب
        return () => {
            window.removeEventListener("beforeunload", handleBeforeUnload);
            sessionStorage.setItem(scrollKey, String(window.scrollY));
        };
    }, [fileId]);

    // جلب بيانات الملف
    const { file, retiree, isLoading, isError, error, refetch } = useFileDetailsQuery(fileId);
    const { confirm, ConfirmDialogElement } = useConfirm();

    // جلب الحالات المسموحة
    const { data: allowedStatuses = [] } = useQuery({
        queryKey: ["allowed-statuses", fileId],
        queryFn: () => retirementApi.getAllowedStatuses(fileId),
        enabled: updateStatusOpen,
    });

    const canDelete = useHasPermission("File.Delete.Soft");

    const deleteMutation = useApiMutation(
        async (id: number) => retirementApi.deleteFile(id),
        {
            successMessage: "تم حذف الملف بنجاح",
            onSuccess: () => {
                navigate("/files");
            }
        }
    );

    const handleDelete = async () => {
        const confirmed = await confirm({
            title: "حذف ملف التقاعد",
            description: "هل أنت متأكد من حذف هذا الملف؟ سيتم نقله إلى سلة المهملات (حذف ناعم). لا يمكن حذف الملفات النشطة.",
            confirmLabel: "نعم، احذف",
            cancelLabel: "إلغاء",
            variant: "danger"
        });

        if (confirmed) {
            deleteMutation.mutate(fileId);
        }
    };



    const isTabVisible = useCallback((tabKey: TabKey) => {
        if (!file) return true;
        if (tabKey === "pledges" && !file.hasPledgesOrExemptions) return false;
        if (tabKey === "settlements" && !file.hasSettlements) return false;

        // Basic Data Tabs (Specific permissions or Auditor)
        if (["entitled", "guardians", "agents", "recipients", "notes"].includes(tabKey)) {
            const hasEntityView = canViewStatement; // Generic view allowed if they can see the file
            const hasEntityEdit = canApprove || hasAnyEntityEdit;

            return hasEntityView || hasEntityEdit;
        }

        // Document Tabs
        if (["documents", "pledges"].includes(tabKey)) {
            return canUploadDocs || canApprove || canEditBasic;
        }

        // Financial Tabs
        if (["entitlements", "salaryPayments", "paymentBooks", "settlements"].includes(tabKey)) {
            return canEditFinancial || canApprove;
        }

        // Review Tabs
        if (["statusTransitions", "review"].includes(tabKey)) {
            return canApprove || canReviewView || canViewHistory;
        }

        return true;
    }, [file, canEditBasic, canApprove, canReviewView, canViewHistory, canEditFinancial, canUploadDocs, canViewStatement, hasAnyEntityEdit]);

    const handleTabChange = useCallback((tab: TabKey) => {
        setActiveTab(tab);
        setSearchParams((prev) => { prev.set("tab", tab); return prev; }, { replace: true });
    }, [setSearchParams]);

    const handleDeathMutation = useApiMutation(
        async (values: RecordDeathFormValues) => {
            return retirementApi.handleDeath(fileId, values.deathDate);
        },
        {
            successMessage: "تم تسجيل وفاة المتقاعد وإيقاف الملف مؤقتاً",
            onSuccess: async () => {
                setRecordDeathOpen(false);
                await queryClient.invalidateQueries({ queryKey: [FILE_DETAIL_KEY, fileId] });
            }
        }
    );

    const correctLifeStatusMutation = useApiMutation(
        async (values: CorrectLifeStatusValues) => retirementApi.correctLifeStatus(fileId, values),
        {
            successMessage: "تم تصحيح حالة المتقاعد وإحالة الملف للمراجعة",
            onSuccess: async () => {
                setCorrectLifeStatusOpen(false);
                await queryClient.invalidateQueries({ queryKey: [FILE_DETAIL_KEY, fileId] });
            }
        }
    );

    const statusMutation = useApiMutation(
        async (values: { newStatusID: number, reason?: string }) => {
            return retirementApi.updateFileStatus(fileId, values.newStatusID, values.reason);
        },
        {
            successMessage: "تم تحديث حالة الملفات بنجاح",
            onSuccess: async () => {
                await queryClient.invalidateQueries({ queryKey: [FILE_DETAIL_KEY, fileId] });
                await queryClient.invalidateQueries({ queryKey: ["status-transitions", fileId] });
                setUpdateStatusOpen(false);
            }
        }
    );

    if (isLoading) return <FileDetailsSkeleton />;
    if (isError) return (
        <div className="flex flex-col items-center justify-center min-h-[60dvh] gap-4">
            <div className="text-destructive font-bold text-lg">حدث خطأ في تحميل بيانات الملف</div>
            <Text variant="description">{error instanceof Error ? error.message : "يرجى المحاولة مرة أخرى"}</Text>
            <Button onClick={() => refetch()}>
                إعادة المحاولة
            </Button>
        </div>
    );
    if (!file || !retiree) return <div                     className="p-6 sm:p-12 text-center font-bold">الملف غير موجود.</div>;

    return (
        <DetailLayout
            stickyHeader
            headerTitle={
                <div className="flex items-start gap-2 sm:gap-3 min-w-0">
                    <Button variant="ghost" size="icon" onClick={() => navigate("/files")} className="rounded-full shrink-0 mt-0.5 h-8 w-8" aria-label="العودة لقائمة الملفات">
                        <ArrowRight className="w-4 h-4 sm:w-5 sm:h-5" />
                    </Button>
                    <div className="flex flex-col text-start min-w-0 flex-1">
                        <Heading level="h1" className={cn(
                            "text-base sm:text-xl font-black tracking-tight leading-tight break-words",
                            getStatusColorClass(file.statusID)
                        )}>
                            {file.retireeFullName}
                        </Heading>
                        <div className="flex items-center gap-1.5 sm:gap-2 flex-wrap mt-0.5">
                            <Badge variant={file.statusID === 2 ? "success" : file.statusID === 6 ? "warning" : "outline"} className="shrink-0 text-[9px] sm:text-xs">
                                {file.fileStatusName}
                            </Badge>
                            <span className="text-[10px] sm:text-sm text-muted-foreground font-medium">ملف رقم <span dir="ltr" className="font-mono">{file.fileNumber}</span></span>
                        </div>
                    </div>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="sm:hidden rounded-xl border h-9 w-9 shrink-0 self-center"
                        onClick={() => setActionsOpen(true)}
                        aria-label="إجراءات إضافية"
                    >
                        <MoreHorizontal className="w-4 h-4" />
                    </Button>
                </div>
            }
            headerActions={
                <div className="flex flex-row items-center gap-1.5 sm:gap-2 w-full sm:w-auto">
                    <PermissionGate permission="File.Edit.FinancialData">
                        <Button
                            variant="outline"
                            className="hidden sm:inline-flex gap-1.5 sm:gap-2 font-bold border-emerald-500 bg-emerald-500/5 text-emerald-700 hover:bg-emerald-100 sm:flex-none whitespace-nowrap h-9 text-[11px] sm:text-sm px-2.5 sm:px-4"
                            onClick={() => setEditFileOpen(true)}
                        >
                            <User className="w-3.5 h-3.5 sm:w-4 sm:h-4 shrink-0" /> <span className="hidden sm:inline">تعديل البيانات الأساسية</span>
                        </Button>
                    </PermissionGate>

                    <PermissionGate permission="File.ChangeStatus.Approve">
                        <Button className="hidden sm:inline-flex gap-1.5 sm:gap-2 font-bold sm:flex-none whitespace-nowrap h-9 text-[11px] sm:text-sm px-2.5 sm:px-4" onClick={() => setUpdateStatusOpen(true)}>
                            <Shield className="w-3.5 h-3.5 sm:w-4 sm:h-4 shrink-0" /> <span className="hidden sm:inline">تغيير الحالة / اعتماد</span>
                        </Button>
                    </PermissionGate>

                    <Button variant="ghost" size="icon" className="hidden sm:inline-flex rounded-xl border h-9 w-9 shrink-0" onClick={() => setActionsOpen(true)} aria-label="إجراءات إضافية">
                        <MoreHorizontal className="w-4 h-4 sm:w-5 sm:h-5" />
                    </Button>
                </div>
            }
            sidebar={<RetirementFileSidebar file={file} retiree={retiree} isCompact={false} />}
        >
            <div className="space-y-2.5 md:space-y-3.5">
                <FileLifecycleActions fileId={fileId} statusId={file.statusID} previousStatusId={file.previousStatusID} reviewStatus={file.reviewStatus} salaryStatus={file.salaryStatus} />
                {(() => {
                    const hasReviewDanger = file?.reviewStatus === 3;
                    const hasReviewWarning = file?.reviewStatus === 1;
                    const hasMissing = file && ((file.missingDocumentsCount || 0) > 0 || (file.missingDataCount || 0) > 0);
                    
                    if (!hasReviewDanger && !hasReviewWarning && !hasMissing) return null;
                    
                    const alertsCount = (hasReviewDanger ? 1 : 0) + (hasReviewWarning ? 1 : 0) + (hasMissing ? 1 : 0);
                    if (alertsCount === 0) return null;

                    return (
                        <Accordion 
                            type="single" 
                            collapsible 
                            defaultValue={hasReviewDanger ? "alerts" : undefined}
                            className={cn(
                                "w-full border rounded-xl overflow-hidden",
                                hasReviewDanger ? "bg-destructive/5 border-destructive" : "bg-amber-50/50 dark:bg-amber-950/20 border-amber-500"
                            )}
                        >
                            <AccordionItem value="alerts" className="border-none">
                                <AccordionTrigger className={cn(
                                    "px-4 py-3 hover:no-underline",
                                    hasReviewDanger ? "text-destructive hover:bg-destructive/10" : "text-amber-700 dark:text-amber-400 hover:bg-amber-500/5"
                                )}>
                                    <div className="flex items-center gap-2">
                                        <AlertTriangle className="w-5 h-5 shrink-0" />
                                        <span className="font-bold">يوجد ({alertsCount}) تنبيهات بخصوص هذا الملف يرجى مراجعتها</span>
                                    </div>
                                </AccordionTrigger>
                                <AccordionContent className="px-4 pb-4">
                                    <div className="flex flex-col gap-3 pt-2">
                                        {hasReviewDanger && (
                                            <AlertBanner
                                                variant="danger"
                                                className="items-start"
                                                message={
                                                    <div className="flex flex-col gap-2">
                                                        <div>
                                                            <strong className="text-sm">تم رفض التعديلات الأخيرة</strong>
                                                            <Text variant="body" className="mt-0.5">يرجى إصلاح الملاحظات التالية ثم إعادة التعديل:</Text>
                                                        </div>
                                                        {file.reviewNotes && (
                                                            <div className="text-sm max-w-3xl bg-destructive/10 p-3 rounded-lg border border-destructive leading-relaxed font-semibold">
                                                                {file.reviewNotes}
                                                            </div>
                                                        )}
                                                    </div>
                                                }
                                                icon={<AlertTriangle className="w-5 h-5 mt-0.5" />}
                                            />
                                        )}

                                        {hasReviewWarning && (
                                            <AlertBanner
                                                variant="warning"
                                                className="items-start sm:items-center"
                                                message={
                                                    <div className="flex flex-col gap-0.5">
                                                        <strong className="text-sm">الملف قيد المراجعة</strong>
                                                        <Text variant="body-sm" className="sm:text-sm">هناك تعديلات على هذا الملف بانتظار المراجعة والاعتماد من قبل التدقيق.</Text>
                                                    </div>
                                                }
                                                icon={<Shield className="w-5 h-5 mt-0.5" />}
                                            />
                                        )}

                                        {hasMissing && (
                                            <AlertBanner
                                                variant="warning"
                                                className="items-start sm:items-center"
                                                message={
                                                    <div className="flex flex-col gap-1.5">
                                                        <div>
                                                            <strong className="text-sm">تنبيه: مستندات وبيانات ناقصة</strong>
                                                            <Text variant="body-sm" className="sm:text-sm mt-0.5">يوجد {file.missingDocumentsCount || 0} وثيقة ناقصة و {file.missingDataCount || 0} حقول بيانات غير مكتملة. يرجى المراجعة لاستكمالها.</Text>
                                                        </div>
                                                        {((file.missingDocumentsList && file.missingDocumentsList.length > 0) || (file.missingDataList && file.missingDataList.length > 0)) && (
                                                            <details className="group mt-1 cursor-pointer">
                                                                <summary className="text-xs font-bold opacity-100 hover:opacity-100 select-none focus-visible:ring-2 focus-visible:ring-primary/20 rounded mb-1">
                                                                    عرض وتفاصيل النواقص
                                                                </summary>
                                                                <div className="flex flex-col gap-2 mt-2 cursor-auto animate-in fade-in duration-300">
                                                                    {file.missingDocumentsList && file.missingDocumentsList.length > 0 && (
                                                                        <div className="text-xs bg-amber-900/10 dark:bg-amber-100/10 p-2.5 rounded-lg border border-amber-900 leading-relaxed">
                                                                            <strong>الوثائق الناقصة:</strong> {file.missingDocumentsList.join("، ")}
                                                                        </div>
                                                                    )}
                                                                    {file.missingDataList && file.missingDataList.length > 0 && (
                                                                        <div className="text-xs bg-amber-900/10 dark:bg-amber-100/10 p-2.5 rounded-lg border border-amber-900 leading-relaxed">
                                                                            <strong>البيانات غير المكتملة:</strong> {file.missingDataList.join("، ")}
                                                                        </div>
                                                                    )}
                                                                </div>
                                                            </details>
                                                        )}
                                                    </div>
                                                }
                                                icon={<AlertTriangle className="w-5 h-5 mt-0.5" />}
                                            />
                                        )}
                                    </div>
                                </AccordionContent>
                            </AccordionItem>
                        </Accordion>
                    );
                })()}
                <FileWorkflowManager
                    file={file}
                    activeTab={activeTab}
                    onTabChange={handleTabChange}
                    isTabVisible={isTabVisible}
                />
            </div>

            <StatusUpdateDialog
                open={updateStatusOpen}
                onOpenChange={setUpdateStatusOpen}
                currentStatusId={file.statusID}
                currentStatusName={file.fileStatusName ?? undefined}
                allowedStatuses={allowedStatuses}
                isLoading={statusMutation.isPending}
                onStatusUpdated={(values) => statusMutation.mutate(values)}
            />

            <RecordDeathDialog
                open={recordDeathOpen}
                onOpenChange={setRecordDeathOpen}
                retireeName={retiree.fullName}
                onSubmit={(v) => handleDeathMutation.mutate(v)}
                isLoading={handleDeathMutation.isPending}
            />

            <CorrectLifeStatusDialog
                open={correctLifeStatusOpen}
                onOpenChange={setCorrectLifeStatusOpen}
                retireeName={retiree.fullName}
                onSubmit={(values) => correctLifeStatusMutation.mutate(values)}
                isLoading={correctLifeStatusMutation.isPending}
            />

            {file && retiree && (
                <RetirementFileDialog
                    mode="edit"
                    open={editFileOpen}
                    onOpenChange={setEditFileOpen}
                    fileId={file.fileID}
                    retireeId={retiree.retireeID}
                    retireeName={retiree.fullName}
                    onSuccess={async () => {
                        await queryClient.invalidateQueries({ queryKey: [FILE_DETAIL_KEY, fileId] });
                    }}
                />
            )}

            <BaseSheet open={actionsOpen} onOpenChange={setActionsOpen} title="إجراءات إضافية">
                <div className="space-y-2 p-4 text-start">
                    <PermissionGate permission="File.Edit.FinancialData">
                        <Button
                            type="button"
                            variant="ghost"
                            className="sm:hidden w-full justify-start gap-3 h-12 rounded-xl text-emerald-700 hover:bg-emerald-50 hover:text-emerald-800"
                            onClick={() => { setActionsOpen(false); setEditFileOpen(true); }}
                            aria-label="تعديل البيانات الأساسية"
                        >
                            <User className="w-5 h-5" /> تعديل البيانات الأساسية
                        </Button>
                    </PermissionGate>

                    <PermissionGate permission="File.ChangeStatus.Approve">
                        <Button
                            type="button"
                            variant="ghost"
                            className="sm:hidden w-full justify-start gap-3 h-12 rounded-xl"
                            onClick={() => { setActionsOpen(false); setUpdateStatusOpen(true); }}
                            aria-label="تغيير الحالة / اعتماد"
                        >
                            <Shield className="w-5 h-5" /> تغيير الحالة / اعتماد
                        </Button>
                    </PermissionGate>

                    {canViewStatement && (
                        <Button
                            type="button"
                            variant="ghost"
                            className="w-full justify-start gap-3 h-12 rounded-xl"
                            onClick={() => { setActionsOpen(false); navigate(`/files/${fileId}/print`); }}
                            aria-label="طباعة ملخص الملف"
                        >
                            <Printer className="w-5 h-5 text-muted-foreground" /> طباعة ملخص الملف
                        </Button>
                    )}

                    <Button
                        type="button"
                        variant="ghost"
                        className="w-full justify-start gap-3 h-12 rounded-xl"
                        onClick={() => { setActionsOpen(false); navigate(`/files/${fileId}/review`); }}
                        aria-label="مراجعة التعديلات"
                    >
                        <FileCheck className="w-5 h-5 text-muted-foreground" /> مراجعة التعديلات
                    </Button>

                    {canViewStatement && (
                        <Button
                            type="button"
                            variant="ghost"
                            className="w-full justify-start gap-3 h-12 rounded-xl"
                            onClick={() => { setActionsOpen(false); navigate(`/retirees/${retiree.retireeID}`); }}
                            aria-label="عرض السجل التاريخي للمتقاعد"
                        >
                            <FileText className="w-5 h-5 text-muted-foreground" /> عرض السجل التاريخي للمتقاعد
                        </Button>
                    )}

                    {file.isAlive && (
                        <PermissionGate permission="File.ChangeStatus.Approve">
                            <Button
                                type="button"
                                variant="ghost"
                                className="w-full justify-start gap-3 h-12 rounded-xl text-destructive hover:bg-destructive/5 hover:text-destructive"
                                onClick={() => { setActionsOpen(false); setRecordDeathOpen(true); }}
                                aria-label="تسجيل وفاة المتقاعد"
                            >
                                <AlertTriangle className="w-5 h-5" /> تسجيل وفاة المتقاعد
                            </Button>
                        </PermissionGate>
                    )}

                    {(!file.isAlive || !!retiree.deathDate || [4, 5].includes(retiree.healthStatusID ?? 0)) && (
                        <PermissionGate permission="File.ChangeStatus.Approve">
                            <Button
                                type="button"
                                variant="ghost"
                                className="w-full justify-start gap-3 h-12 rounded-xl"
                                onClick={() => { setActionsOpen(false); setCorrectLifeStatusOpen(true); }}
                            >
                                <ClipboardCheck className="w-5 h-5" /> تصحيح بيانات حالة الحياة
                            </Button>
                        </PermissionGate>
                    )}

                    {canDelete && (
                        <Button
                            type="button"
                            variant="ghost"
                            className="w-full justify-start gap-3 h-12 rounded-xl text-destructive hover:bg-destructive/5 hover:text-destructive"
                            onClick={() => handleDelete()}
                            aria-label="حذف الملف"
                        >
                            <Trash2 className="w-5 h-5" /> حذف الملف
                        </Button>
                    )}
                </div>
            </BaseSheet>
            {ConfirmDialogElement}
        </DetailLayout>
    );
}

function FileDetailsSkeleton() {
    return (
        <div className="space-y-8 text-start" dir="rtl">
            <div className="flex justify-between items-center flex-wrap">
                <div className="flex gap-4">
                    <Skeleton className="h-10 w-10 rounded-full" />
                    <div className="space-y-2">
                        <Skeleton className="h-8 w-full max-w-[200px]" />
                        <Skeleton className="h-4 w-full max-w-[160px]" />
                    </div>
                </div>
                <div className="flex gap-2 flex-wrap">
                    <Skeleton className="h-10 w-32 rounded-xl" />
                    <Skeleton className="h-10 w-40 rounded-xl" />
                </div>
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
                <div className="lg:col-span-1 space-y-6">
                    <Skeleton className="h-48 sm:h-96 rounded-3xl" />
                </div>
                <div className="lg:col-span-3 space-y-6">
                    <Skeleton className="h-14 rounded-2xl" />
                    <Skeleton className="h-[300px] sm:h-[500px] rounded-3xl" />
                </div>
            </div>
        </div>
    );
}
