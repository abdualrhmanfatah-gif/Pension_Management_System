<<<<<<< HEAD
import { DollarSign, FileText, History,UserCheck, Users } from "lucide-react";
import { useState } from "react";
import { useParams, useSearchParams } from "react-router-dom";

import { retirementApi } from "@/api/retirement";
import { FileReviewHeader } from "@/components/domain/retirement/FileReviewHeader";
import { FileReviewSidebar } from "@/components/domain/retirement/FileReviewSidebar";
import { ReviewPanel } from "@/components/domain/retirement/ReviewPanel";
import { BasicInfoSection } from "@/components/domain/retirement/sections/BasicInfoSection";
import { DocumentsSection } from "@/components/domain/retirement/sections/DocumentsSection";
import { FinancialSection } from "@/components/domain/retirement/sections/FinancialSection";
import { HistorySection } from "@/components/domain/retirement/sections/HistorySection";
import { PeopleSection } from "@/components/domain/retirement/sections/PeopleSection";
import { Button, Skeleton, Text, Textarea } from "@/components/ui";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { REVIEW_SECTIONS, REVIEW_SECTIONS_AR } from "@/constants";
import { useFileDetailsQuery } from "@/hooks/queries";
import { useApiMutation } from "@/hooks/useApiMutation";
import { cn } from "@/lib/utils";
import type { ReviewSection } from "@/types/retirement";
=======
import { useState } from "react";
import { useParams, useSearchParams } from "react-router-dom";
import { Users, UserCheck, FileText, DollarSign, History } from "lucide-react";

import { useFileDetailsQuery } from "@/hooks/queries";
import { Button, Skeleton, Text, Textarea } from "@/components/ui";
import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { FileReviewSidebar } from "@/components/domain/retirement/FileReviewSidebar";
import { FileReviewHeader } from "@/components/domain/retirement/FileReviewHeader";
import { ReviewPanel } from "@/components/domain/retirement/ReviewPanel";
import { BasicInfoSection } from "@/components/domain/retirement/sections/BasicInfoSection";
import { PeopleSection } from "@/components/domain/retirement/sections/PeopleSection";
import { DocumentsSection } from "@/components/domain/retirement/sections/DocumentsSection";
import { FinancialSection } from "@/components/domain/retirement/sections/FinancialSection";
import { HistorySection } from "@/components/domain/retirement/sections/HistorySection";
import { useApiMutation } from "@/hooks/useApiMutation";
import { retirementApi } from "@/api/retirement";
import { cn } from "@/lib/utils";
import type { ReviewSection } from "@/types/retirement";
import { REVIEW_SECTIONS, REVIEW_SECTIONS_AR } from "@/constants";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1


export function FileReviewPage() {
    const { id } = useParams<{ id: string }>();
    const fileId = parseInt(id || "0");

    const { file, retiree, isLoading, isError, error } = useFileDetailsQuery(fileId);

    const [searchParams, setSearchParams] = useSearchParams();
    const tabFromUrl = searchParams.get("tab") as ReviewSection | null;
    const validSections: ReviewSection[] = ["basic", "people", "documents", "financial", "history"];

    const [activeSection, setActiveSection] = useState<ReviewSection>(
        tabFromUrl && validSections.includes(tabFromUrl) ? tabFromUrl : "basic"
    );
    const [reviewPanelOpen, setReviewPanelOpen] = useState(false);
    const [rejectDialogOpen, setRejectDialogOpen] = useState(false);
    const [rejectReason, setRejectReason] = useState("");

    const handleSectionChange = (section: ReviewSection) => {
        setActiveSection(section);
        setSearchParams((prev) => {
            prev.set("tab", section);
            return prev;
        }, { replace: true });
    };


    const approveMutation = useApiMutation(
        async () => {
<<<<<<< HEAD
            await retirementApi.approveReview(fileId);
=======
            await retirementApi.updateFileStatus(fileId, 2, "اعتماد الملف");
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        },
        {
            invalidateKeys: [["retirement-file", fileId], ["review-items", fileId]],
            successMessage: "تم اعتماد الملف بنجاح",
        }
    );

    const rejectMutation = useApiMutation(
        async () => {
<<<<<<< HEAD
            await retirementApi.rejectReview(fileId, rejectReason);
        },
        {
            invalidateKeys: [["retirement-file", fileId], ["review-items", fileId]],
            successMessage: "رُفضت المراجعة؛ يبقى الملف معلقًا والصرف موقوفًا",
=======
            await retirementApi.updateFileStatus(fileId, 3, rejectReason || "رفض الملف");
        },
        {
            invalidateKeys: [["retirement-file", fileId], ["review-items", fileId]],
            successMessage: "تم رفض الملف بنجاح",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            onSuccess: () => {
                setRejectDialogOpen(false);
                setRejectReason("");
            },
        }
    );

    const handleReject = () => {
        setRejectDialogOpen(true);
    };

    const handleConfirmReject = () => {
        rejectMutation.mutate(undefined);
    };

    if (isLoading) {
        return (
            <div className="flex flex-col h-full">
                <header className="bg-card border-b border-border px-6 py-3 flex-shrink-0">
                    <Skeleton className="h-10 w-full" />
                </header>
                <div className="flex flex-1">
                    <aside className="hidden md:block md:w-64 border-l border-border p-4">
                        <Skeleton className="h-full" />
                    </aside>
                    <main className="flex-1 p-6">
                        <Skeleton className="h-full" />
                    </main>
                </div>
            </div>
        );
    }

    if (isError || !file) {
        return (
            <div className="flex items-center justify-center h-full">
                <div className="text-center">
                    <Text variant="body" className="font-bold text-destructive">حدث خطأ</Text>
                    <Text variant="description">{(error as Error)?.message || "الملف غير موجود"}</Text>
                </div>
            </div>
        );
    }

    const missingDocsCount = file.missingDocumentsCount || 0;
    const missingDataCount = file.missingDataCount || 0;
    const pendingReviewCount = file.reviewCount ?? 0;

    const navItems: { key: ReviewSection; icon: typeof Users; label: string }[] = [
        { key: REVIEW_SECTIONS.BASIC, icon: Users, label: REVIEW_SECTIONS_AR[REVIEW_SECTIONS.BASIC] },
        { key: REVIEW_SECTIONS.PEOPLE, icon: UserCheck, label: REVIEW_SECTIONS_AR[REVIEW_SECTIONS.PEOPLE] },
        { key: REVIEW_SECTIONS.DOCUMENTS, icon: FileText, label: REVIEW_SECTIONS_AR[REVIEW_SECTIONS.DOCUMENTS] },
        { key: REVIEW_SECTIONS.FINANCIAL, icon: DollarSign, label: REVIEW_SECTIONS_AR[REVIEW_SECTIONS.FINANCIAL] },
        { key: REVIEW_SECTIONS.HISTORY, icon: History, label: REVIEW_SECTIONS_AR[REVIEW_SECTIONS.HISTORY] },
    ];

    return (
        <div className="flex flex-col h-full bg-background">
            <header className="bg-card border-b border-border px-6 py-3 flex-shrink-0">
                <FileReviewHeader
                    fileId={fileId}
                    fileNumber={file.fileNumber}
                    retireeName={file.retireeFullName}
                    statusID={file.statusID ?? null}
                    statusName={file.fileStatusName}
                    pendingReviewCount={pendingReviewCount}
                    missingDocsCount={missingDocsCount}
                    onToggleReviewPanel={() => setReviewPanelOpen(!reviewPanelOpen)}
                    onApprove={() => approveMutation.mutate(undefined)}
                    isApproving={approveMutation.isPending}
                    onReject={handleReject}
                    isRejecting={rejectMutation.isPending}
                />
            </header>

            {/* ─── شريط التنقل الأفقي ─── */}
            <nav className="bg-card border-b border-border px-6 flex-shrink-0 overflow-x-auto">
                <div className="flex items-center gap-1 min-w-max">
                    {navItems.map((item) => {
                        const Icon = item.icon;
                        const isActive = activeSection === item.key;
                        return (
                            <Button
                                key={item.key}
                                variant="ghost"
                                className={`justify-start gap-2 px-4 py-3 h-auto border-b-2 ${isActive ? "border-primary text-primary" : "border-transparent text-muted-foreground"}`}
                                onClick={() => handleSectionChange(item.key)}
                            >
                                <Icon className="w-4 h-4" />
                                {item.label}
                                {item.key === REVIEW_SECTIONS.DOCUMENTS && missingDocsCount > 0 && (
                                    <span className="bg-destructive text-destructive-foreground text-[10px] px-1.5 py-0.5 rounded-full font-bold">
                                        {missingDocsCount}
                                    </span>
                                )}
                            </Button>
                        );
                    })}
                </div>
            </nav>

            <div className="flex flex-1 overflow-hidden">
                <aside className="hidden lg:block lg:w-72 bg-card border-l border-border flex-shrink-0 overflow-y-auto overflow-x-hidden">
                    <FileReviewSidebar
                        file={file}
                        retiree={retiree!}
                        activeSection={activeSection}
                        onSectionChange={handleSectionChange}
                        missingDocsCount={missingDocsCount}
                        missingDataCount={missingDataCount}
                        pendingReviewCount={pendingReviewCount}
                    />
                </aside>

                <main className={cn("flex-1 p-6 flex flex-col min-h-0", activeSection === "documents" ? "overflow-hidden" : "overflow-y-auto")}>
                    {activeSection === "basic" && (
                            <BasicInfoSection
                                file={file}
                                retiree={retiree!}
                            fileId={fileId}
                            missingDocsCount={missingDocsCount}
                            missingDataCount={missingDataCount}
                            pendingReviewCount={pendingReviewCount}
                        />
                    )}
                    {activeSection === "people" && (
                        <PeopleSection fileId={fileId} fileNumber={file.fileNumber} />
                    )}
                    {activeSection === "documents" && (
                        <DocumentsSection fileId={fileId} />
                    )}
                    {activeSection === "financial" && (
                        <FinancialSection fileId={fileId} retireeId={file.retireeID} />
                    )}
                    {activeSection === "history" && (
                        <HistorySection fileId={fileId} />
                    )}
                </main>

                <ReviewPanel
                    fileId={fileId}
                    isOpen={reviewPanelOpen}
                    onClose={() => setReviewPanelOpen(false)}
                />
            </div>

            <BaseFormDialog
                isOpen={rejectDialogOpen}
                onClose={() => {
                    setRejectDialogOpen(false);
                    setRejectReason("");
                }}
                onSubmit={handleConfirmReject}
                title="رفض الملف"
                description="يرجى إدخال سبب رفض الملف"
                isLoading={rejectMutation.isPending}
                submitLabel="رفض الملف"
            >
                <div className="space-y-4">
                    <div>
                        <Text variant="label" className="mb-1.5 block">ملاحظات الرفض</Text>
                        <Textarea
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            placeholder="أدخل سبب رفض الملف..."
                            rows={4}
                        />
                    </div>
                </div>
            </BaseFormDialog>
        </div>
    );
}
