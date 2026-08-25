import { useQuery } from "@tanstack/react-query";
import {
    AlertTriangle,
    CheckCircle,
    Clock,
    Link2,
    ShieldAlert,
    XCircle,
} from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { complianceApi } from "@/api/compliance";
import { Badge, Button, Card, DataTable, type CustomColumnDef, ErrorState, Heading, PageLayout, StatCard, Text, StickyHeader } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { cn } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Types ──────────────────────────────────────────────

interface ComplianceDashboard {
    totalOpenRiskEvents: number;
    criticalRiskEvents: number;
    highRiskEvents: number;
    pendingOverrides: number;
    recentRiskEvents: RiskEventSummary[];
    pendingOverrideReviews: OverrideReview[];
    branchRiskSummary: BranchRisk[];
}

interface RiskEventSummary {
    id: number;
    fileId: number;
    riskType: string;
    severity: string;
    riskScore: number;
    description: string;
    raisedAt: string;
    status: string;
}

interface OverrideReview {
    logId: number;
    fileId: number;
    actorId: number;
    actorName: string;
    action: string;
    actionDate: string;
    reason: string | null;
}

interface BranchRisk {
    branchId: number;
    branchName: string;
    openRiskCount: number;
    totalPendingTasks: number;
}

// ─── Page Component ─────────────────────────────────────

export function ComplianceDashboardPage() {
    const [chainResult, setChainResult] = useState<{ isIntact: boolean; failedRecords: number; message: string } | null>(null);
    const canManage = useHasPermission("System.Admin", "Audit.ViewAll");

    const { data: dashboard, isLoading, error } = useQuery<ComplianceDashboard>({
        queryKey: ["compliance-dashboard"],
        queryFn: () => complianceApi.getDashboard(),
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل بيانات لوحة الامتثال", { id: "compliance-dashboard" });
    }, [error]);

    const verifyChain = useApiMutation(async () => {
        const res = await complianceApi.verifyChain();
        return res;
    }, {
        successMessage: "تم الانتهاء من فحص سلسلة التدقيق",
        onSuccess: (data) => setChainResult(data)
    });

    const resolveRisk = useApiMutation(async (id: number) => {
        await complianceApi.resolveRisk(id);
    }, {
        successMessage: "تم حل حدث المخاطر بنجاح",
        invalidateKeys: [["compliance-dashboard"]]
    });

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-[60dvh]">
                <div className="text-muted-foreground">جاري التحميل...</div>
            </div>
        );
    }

    if (error) {
        return (
            <ErrorState message="تعذر تحميل بيانات لوحة الامتثال" />
        );
    }

    if (!dashboard) return null;

    return (
        <PageLayout>
            {/* ── Header ──────────────────────────── */}
            <StickyHeader>
                <div className="flex items-center justify-between flex-wrap gap-3">
                    <div>
                        <Heading level="h1">لوحة الامتثال والرقابة</Heading>
                        <Text variant="description" className="mt-1">مراقبة المخاطر والتجاوزات وسلامة النظام</Text>
                    </div>
                    {canManage && (
                        <Button
                            onClick={() => verifyChain.mutate(undefined)}
                            disabled={verifyChain.isPending}
                        >
                            <Link2 className="h-4 w-4" />
                            {verifyChain.isPending ? "جاري الفحص..." : "فحص سلامة سلسلة التدقيق"}
                        </Button>
                    )}
                </div>
            </StickyHeader>

            {/* ── Chain Integrity Result ───────────── */}
            {chainResult && (
                <div className={cn(
                    "flex items-center gap-3 p-4 rounded-xl border",
                    chainResult.isIntact
                        ? "bg-emerald-500/5 border-emerald-500/20 text-emerald-400"
                        : "bg-red-500/5 border-red-500/20 text-red-400"
                )}>
                    {chainResult.isIntact ? <CheckCircle className="h-5 w-5" /> : <XCircle className="h-5 w-5" />}
                    <span className="text-sm font-medium">{chainResult.message}</span>
                </div>
            )}

            {/* ── Stats Cards ─────────────────────── */}
            <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 gap-2 sm:gap-4">
                <StatCard title="أحداث مخاطر مفتوحة" value={dashboard.totalOpenRiskEvents} icon={<AlertTriangle />} variant="orange" />
                <StatCard title="أحداث حرجة" value={dashboard.criticalRiskEvents} icon={<ShieldAlert />} variant="destructive" />
                <StatCard title="تجاوزات معلقة" value={dashboard.pendingOverrides || dashboard.pendingOverrideReviews?.length || 0} icon={<Clock />} variant="amber" />
            </div>

            {/* ── Risk Events Table ───────────────── */}
            <Card className="p-5">
                <Heading level="h2" className="text-lg font-bold text-foreground mb-4">أحدث أحداث المخاطر</Heading>
                {!dashboard.recentRiskEvents?.length ? (
                    <Text variant="description" className="text-center py-8">لا توجد أحداث مخاطر مفتوحة</Text>
                ) : (
                    <DataTable
                        columns={[
                            { key: "fileId", header: "الملف", render: (row) => <span className="font-mono text-xs">#{row.fileId}</span> },
                            { key: "riskType", header: "النوع", accessorFn: (row) => row.riskType },
                            { key: "severity", header: "الخطورة", render: (row) => <Badge variant={row.severity === "Critical" ? "destructive" : row.severity === "High" ? "warning" : row.severity === "Medium" ? "default" : "info"}>{row.severity}</Badge> },
                            { key: "riskScore", header: "الدرجة", render: (row) => <span className="font-bold">{row.riskScore}</span> },
                            { key: "description", header: "الوصف", render: (row) => <span className="text-muted-foreground max-w-xs truncate">{row.description}</span> },
                            { key: "raisedAt", header: "التاريخ", render: (row) => <span className="text-xs text-muted-foreground">{new Date(row.raisedAt).toLocaleDateString("ar-YE")}</span> },
                            ...(canManage ? [{ key: "action", header: "إجراء", render: (row: RiskEventSummary) => <Button size="sm" variant="ghost" onClick={() => resolveRisk.mutate(row.id)} disabled={resolveRisk.isPending}>حل</Button> }] : []),
                        ] as CustomColumnDef<RiskEventSummary>[]}
                        data={dashboard.recentRiskEvents}
                        rowKey={(row) => row.id}
                    />
                )}
            </Card>

            {/* ── Pending Overrides Table ─────────── */}
            <Card className="p-5">
                <Heading level="h2" className="text-lg font-bold text-foreground mb-4">تجاوزات الصلاحيات المعلقة</Heading>
                {!dashboard.pendingOverrideReviews || dashboard.pendingOverrideReviews.length === 0 ? (
                    <Text variant="description" className="text-center py-8">لا توجد تجاوزات معلقة للمراجعة</Text>
                ) : (
                    <DataTable
                        columns={[
                            { key: "fileId", header: "رقم الملف", render: (row) => <span className="font-mono text-xs">#{row.fileId}</span> },
                            { key: "actorName", header: "المدير المخالف", render: (row) => <span className="font-medium">{row.actorName || `User ID: ${row.actorId}`}</span> },
                            { key: "action", header: "الإجراء", accessorFn: (row) => row.action },
                            { key: "actionDate", header: "التاريخ", render: (row) => <span className="text-xs text-muted-foreground">{new Date(row.actionDate).toLocaleDateString("ar-YE")}</span> },
                            { key: "reason", header: "السبب / القيم الجديدة", render: (row) => <span className="text-muted-foreground max-w-xs truncate">{row.reason || "لا يوجد مبرر"}</span> },
                        ] as CustomColumnDef<OverrideReview>[]}
                        data={dashboard.pendingOverrideReviews}
                        rowKey={(row) => row.logId}
                    />
                )}
            </Card>

            {/* ── Branch Risk Summary ─────────────── */}
            {(dashboard.branchRiskSummary?.length ?? 0) > 0 && (
                <Card className="p-5">
                    <Heading level="h2" className="text-lg font-bold text-foreground mb-4">ملخص مخاطر الفروع</Heading>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                        {dashboard.branchRiskSummary.map((branch) => (
                            <div key={branch.branchId} className="flex items-center justify-between p-4 rounded-xl bg-muted/30 border border-border/50">
                                <div>
                                    <p className="font-medium text-foreground">{branch.branchName}</p>
                                    <Text variant="caption" className="mt-1">{branch.totalPendingTasks} مهمة معلقة</Text>
                                </div>
                                <div className="text-end">
                                    <Text variant="body" className="font-bold text-red-400">{branch.openRiskCount} مخاطر</Text>
                                </div>
                            </div>
                        ))}
                    </div>
                </Card>
            )}
        </PageLayout>
    );
}


