import { useQuery } from "@tanstack/react-query";
import { 
    ArrowUpRight,
    Briefcase, 
    Building2, 
    FileText, 
    History,
    LayoutDashboard,
    ShieldCheck,
    TrendingUp} from "lucide-react";
import { useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";

import type { ApprovalRequestDto,PortfolioDto } from "@/api/investment";
import { investmentApi } from "@/api/investment";
import { DashboardHeader } from "@/components/common/dashboards";
import { StatCard } from "@/components/ui";
import { Badge, Button,Card, ErrorState, Heading, PageLayout, Skeleton } from "@/components/ui";
import { APPROVAL_STATUS,BADGE_VARIANT } from "@/constants";
import { formatCurrency, formatDate, formatNumber } from "@/lib/utils";

/**
 * لوحة التحكم الرئيسية للاستثمارات
 * تعرض ملخصاً للمحافظ والأصول والعمليات الجارية
 */
export function InvestmentHub() {
    const navigate = useNavigate();
    // جلب الإحصائيات (الجديدة)
    const { data: stats, isLoading: statsLoading, error } = useQuery({
        queryKey: ["investment", "stats"],
        queryFn: () => investmentApi.getStats()
    });

    const { data: portfoliosRes, isLoading: portfoliosLoading } = useQuery({
        queryKey: ["investment", "portfolios"],
        queryFn: () => investmentApi.getPortfolios()
    });

    const { data: approvalsRes, isLoading: approvalsLoading } = useQuery({
        queryKey: ["investment", "approvals"],
        queryFn: () => investmentApi.getApprovals()
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل لوحة الاستثمارات", { id: "investment-hub" });
    }, [error]);

    // جلب المحافظ (تم الاحتفاظ بها للقائمة)
    const portfolios = portfoliosRes || [];
    const approvals = approvalsRes || [];

    if (error) {
        return <ErrorState message="تعذر تحميل لوحة الاستثمارات" />;
    }

    return (
        <PageLayout className="text-start">
            <DashboardHeader
                title="نظرة عامة على الاستثمارات"
                description="متابعة المحافظ الاستثمارية والأصول والعمليات التشغيلية والاعتمادات المالية."
                icon={<LayoutDashboard className="h-6 w-6" />}
                actions={
                    <Button variant="outline" className="gap-2 rounded-xl h-11 font-bold" onClick={() => navigate("/investment/portfolios")}>
                        <Briefcase className="w-4 h-4" /> إدارة المحافظ
                    </Button>
                }
            />

            {/* ── الإحصائيات المختصرة ────────────────────────── */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard 
                    title="إجمالي المحافظ" 
                    value={formatNumber(stats?.totalPortfolios || 0)} 
                    icon={<Briefcase className="h-5 w-5" />} 
                    variant="blue"
                    isLoading={statsLoading}
                />
                <StatCard 
                    title="إجمالي قيمة الأصول" 
                    value={formatCurrency(stats?.totalAssetsValue || 0)} 
                    icon={<Building2 className="h-5 w-5" />} 
                    variant="emerald"
                    isLoading={statsLoading}
                />
                <StatCard 
                    title="عقود التأجير النشطة" 
                    value={formatNumber(stats?.activeLeaseContracts || 0)} 
                    icon={<FileText className="h-5 w-5" />} 
                    variant="cyan"
                    isLoading={statsLoading}
                />
                <StatCard 
                    title="اعتمادات معلقة" 
                    value={formatNumber(stats?.pendingApprovalsCount || 0)} 
                    icon={<ShieldCheck className="h-5 w-5" />} 
                    variant="orange"
                    description={`${stats?.openMaintenanceRequests || 0} طلب صيانة مفتوح`}
                    isLoading={statsLoading}
                />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* ── أحدث المحافظ ──────────────────── */}
                <Card className="lg:col-span-2 overflow-hidden border border-2">
                    <div className="p-6 border-b flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <TrendingUp className="w-5 h-5 text-primary" />
                            <Heading level="h2" className="font-bold text-lg">أحدث المحافظ الاستثمارية</Heading>
                        </div>
                        <Link to="/investment/portfolios" className="text-sm text-primary hover:underline flex items-center gap-1 font-bold">
                            عرض الكل <ArrowUpRight className="w-4 h-4" />
                        </Link>
                    </div>
                    <div className="divide-y text-start">
                        {portfoliosLoading ? (
                            Array(3).fill(0).map((_, i) => <div key={i} className="p-4"><Skeleton className="h-12 w-full rounded-xl" /></div>)
                        ) : portfolios.length === 0 ? (
                            <div className="p-6 sm:p-12 text-center text-muted-foreground italic">لا توجد محافظ استثمارية مسجلة حالياً</div>
                        ) : (
                            portfolios.slice(0, 5).map((p: PortfolioDto) => (
                                <Link 
                                    key={p.portfolioID} 
                                    to={`/investment/portfolios/${p.portfolioID}`}
                                    className="flex items-center justify-between p-4 hover:bg-primary/[0.02] transition-colors group flex-wrap gap-2"
                                >
                                    <div className="flex items-center gap-4">
<<<<<<< HEAD
                                        <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-white transition-all  border border-primary">
=======
                                        <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-white transition-all  border border-primary/5">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            <Briefcase className="w-5 h-5" />
                                        </div>
                                        <div className="text-start">
                                            <p className="font-bold text-foreground group-hover:text-primary transition-colors">{p.name}</p>
                                            <p className="text-[10px] sm:text-xs text-muted-foreground font-medium">المدير: {p.managerName}</p>
                                        </div>
                                    </div>
                                    <div className="text-end">
                                        <p className="text-sm font-black text-primary">{formatCurrency(p.totalTargetCapital)}</p>
                                        <p className="text-[9px] text-muted-foreground font-bold uppercase tracking-tighter">رأس مال مستهدف</p>
                                    </div>
                                </Link>
                            ))
                        )}
                    </div>
                </Card>

                {/* ── طلبات الاعتماد الأخيرة ───────── */}
                <Card className="overflow-hidden border border-2">
                    <div className="p-6 border-b flex items-center gap-2">
                        <History className="w-5 h-5 text-primary" />
                        <Heading level="h2" className="font-bold text-lg">طلبات الاعتماد</Heading>
                    </div>
                    <div className="p-4 space-y-4 max-h-[50vh] sm:max-h-[450px] overflow-y-auto custom-scrollbar">
                        {approvalsLoading ? (
                            Array(4).fill(0).map((_, i) => <Skeleton key={i} className="h-20 w-full rounded-xl" />)
                        ) : approvals.length === 0 ? (
                            <div className="py-12 text-center text-muted-foreground text-sm italic">لا توجد طلبات معلقة حالياً</div>
                        ) : (
                            approvals.slice(0, 6).map((app: ApprovalRequestDto) => (
<<<<<<< HEAD
                                <div key={app.approvalID} className="flex flex-col gap-2 p-4 rounded-xl border bg-muted/20 hover:border-primary transition-all text-start group">
                                    <div className="flex items-center justify-between">
                                        <Badge variant={BADGE_VARIANT.OUTLINE} className="text-[10px] sm:text-xs px-2 h-5 bg-background font-bold border-primary">
=======
                                <div key={app.approvalID} className="flex flex-col gap-2 p-4 rounded-xl border bg-muted/20 hover:border-primary/30 transition-all text-start group">
                                    <div className="flex items-center justify-between">
                                        <Badge variant={BADGE_VARIANT.OUTLINE} className="text-[10px] sm:text-xs px-2 h-5 bg-background font-bold border-primary/10">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            {app.entityType}
                                        </Badge>
                                        <Badge 
                                            variant={
                                                app.status === APPROVAL_STATUS.APPROVED ? BADGE_VARIANT.SUCCESS : 
                                                app.status === APPROVAL_STATUS.PENDING ? BADGE_VARIANT.WARNING : 
                                                BADGE_VARIANT.DANGER
                                            }
                                            className="text-[9px] font-black h-5"
                                        >
                                            {app.status === APPROVAL_STATUS.APPROVED ? 'معتمد' : app.status === APPROVAL_STATUS.PENDING ? 'قيد الانتظار' : 'مرفوض'}
                                        </Badge>
                                    </div>
                                    <p className="text-xs font-black text-foreground group-hover:text-primary transition-colors">طلب اعتماد #{app.entityID}</p>
<<<<<<< HEAD
                                    <div className="flex items-center justify-between text-[10px] sm:text-xs text-muted-foreground font-medium border-t border-border pt-2 mt-1">
=======
                                    <div className="flex items-center justify-between text-[10px] sm:text-xs text-muted-foreground font-medium border-t border-border/50 pt-2 mt-1">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        <span>بواسطة: {app.requestedByUserName}</span>
                                        <span className="font-mono">{formatDate(app.createdAt)}</span>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </Card>
            </div>
        </PageLayout>
    );
}
