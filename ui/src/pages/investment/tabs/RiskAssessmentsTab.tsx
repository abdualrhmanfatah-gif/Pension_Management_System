import { useQuery, useQueryClient } from "@tanstack/react-query";
import { FileText, FileWarning } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import type { RiskAssessmentDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { InvestmentDocsViewerDialog } from "@/components/domain/investment/InvestmentDocsViewerDialog";
import { RiskAssessmentDialog } from "@/components/domain/investment/RiskAssessmentDialog";
import { Badge, Button, CrudPageLayout, ErrorState } from "@/components/ui";
import { type CustomColumnDef } from "@/components/ui/DataTable";
import { useCrudState } from "@/hooks";
import { useHasPermission } from "@/stores/authStore";

interface RiskAssessmentsTabProps {
    projectId?: number;
}

/**
 * تبويب سجل تقييم المخاطر للمشروع أو النظام
 */
export function RiskAssessmentsTab({ projectId }: RiskAssessmentsTabProps) {
    const canApprove = useHasPermission("Investment.Approve");
    const crud = useCrudState<RiskAssessmentDto>();
    const queryClient = useQueryClient();

    // جلب البيانات
    const { data: risksData, isLoading, error } = useQuery({
        queryKey: projectId ? ["project-risks", projectId] : ["investment-risks"],
        queryFn: () => investmentApi.getRisks(projectId || 0)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل تقييم المخاطر", { id: "risk-assessments" });
    }, [error]);

    const risks = risksData || [];

    const getScoreBadge = (score: number) => {
        if (score >= 4) return <Badge variant="destructive" className="rounded-full px-3">{score} - مرتفع جداً</Badge>;
        if (score === 3) return <Badge variant="warning" className="rounded-full px-3">{score} - متوسط</Badge>;
        return <Badge variant="success" className="rounded-full px-3">{score} - منخفض</Badge>;
    };

    const getRiskTypeLabel = (type?: string) => {
        switch (type) {
            case "Financial": return "مالي";
            case "Operational": return "تشغيلي";
            case "Legal": return "قانوني";
            case "Market": return "سوقي";
            default: return type || "غير محدد";
        }
    };

    const columns = useMemo<CustomColumnDef<RiskAssessmentDto>[]>(() => {
        const cols: CustomColumnDef<RiskAssessmentDto>[] = [
            {
                key: "assessedDate",
                header: "تاريخ التقييم",
                render: (row: RiskAssessmentDto) => <span className="text-sm font-medium">{new Date(row.assessedDate).toLocaleDateString("ar-YE")}</span>
            },
            {
                key: "riskType",
                header: "نوع المخاطرة",
                render: (row: RiskAssessmentDto) => (
                    <div className="flex items-center gap-2">
                        <FileWarning className="w-3.5 h-3.5 text-muted-foreground" />
                        <span>{getRiskTypeLabel(row.riskType)}</span>
                    </div>
                )
            },
            {
                key: "riskScore",
                header: "درجة الخطورة",
                render: (row: RiskAssessmentDto) => getScoreBadge(row.riskScore)
            },
            {
                key: "mitigationPlan",
                header: "خطة المعالجة",
                render: (row: RiskAssessmentDto) => (
                    <div className="max-w-[40vw] sm:max-w-[300px] truncate text-muted-foreground text-sm text-start" title={row.mitigationPlan}>
                        {row.mitigationPlan}
                    </div>
                )
            }
        ];

        if (!projectId) {
            cols.splice(cols.length - 1, 0, {
                key: "projectID",
                header: "المشروع",
                accessor: (row: RiskAssessmentDto) => row.projectID
            });
        }

        cols.push({
            key: "actions",
            header: "الوثائق",
            render: (row: RiskAssessmentDto) => (
                <Button variant="ghost" size="icon" onClick={() => crud.openViewer(row)} title="الوثائق">
                    <FileText className="w-4 h-4 text-primary" />
                </Button>
            )
        });

        return cols;
    }, [projectId, crud]);

    if (error) {
        return <ErrorState message="خطأ في تحميل تقييمات المخاطر" />;
    }

    return (
        <CrudPageLayout
            title="سجل تقييم المخاطر"
            description="تحليل ومعالجة المخاطر المحتملة للمشاريع الاستثمارية"
            createLabel="إضافة تقييم"
            onCreateClick={canApprove ? crud.openCreate : undefined}
            canCreate={canApprove}
            columns={columns}
            data={risks}
            loading={isLoading}
            emptyMessage="لم يتم تسجيل أي مخاطر حتى الآن."
        >
            <RiskAssessmentDialog
                open={crud.isFormOpen}
                onOpenChange={crud.closeForm}
                projectId={projectId || 0}
                onSuccess={() => { queryClient.invalidateQueries({ queryKey: projectId ? ["project-risks", projectId] : ["investment-risks"] }); }}
            />

            <InvestmentDocsViewerDialog
                open={crud.isViewerOpen}
                onOpenChange={crud.closeViewer}
                entityType="risk-assessment"
                entityId={crud.viewerTarget?.riskID || 0}
                title={`وثائق تقييم المخاطر (ID: ${crud.viewerTarget?.riskID})`}
            />
        </CrudPageLayout>
    );
}
