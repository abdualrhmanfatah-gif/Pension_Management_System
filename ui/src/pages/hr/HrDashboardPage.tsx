import { useQuery } from "@tanstack/react-query";
import {
  ArrowUpRight,
  CalendarDays,
  CheckCircle2,
  Clock,
  LayoutDashboard,
  Plus,
  Users} from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { hrApi, type LeaveRequestListItemDto } from "@/api/hr";
import { DashboardHeader } from "@/components/common/dashboards";
import { CreateEmployeeDialog } from "@/components/domain/hr/CreateEmployeeDialog";
import { StatCard } from "@/components/ui";
import { Badge, Button, Card, Heading, PageLayout, Skeleton } from "@/components/ui";
import { LEAVE_STATUS } from "@/constants";
import { formatDate, formatNumber } from "@/lib/utils";

/**
 * لوحة التحكم الرئيسية لإدارة الموارد البشرية
 */
export function HrDashboardPage() {
  const navigate = useNavigate();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  // إحصائيات الموظفين (الجديدة)
  const { data: stats, isLoading: loadingStats } = useQuery({
    queryKey: ["hr", "stats"],
    queryFn: () => hrApi.getStats()
  });

  // طلبات الإجازات الأخيرة
  const { data: leaves, isLoading: loadingLeaves } = useQuery({
    queryKey: ["hr", "leaves", { pageSize: 5 }],
    queryFn: () => hrApi.getLeaveRequests({ pageSize: 5 })
  });

  return (
    <PageLayout className="text-start">
      {/* Page Header */}
      <DashboardHeader
        title="لوحة الشؤون الإدارية"
        description="نظرة عامة شاملة على القوى العاملة والعمليات الإدارية النشطة"
        icon={<LayoutDashboard className="h-6 w-6" />}
        actions={
          <>
            <Button variant="outline" className="gap-2 border-primary/20 bg-primary/5 hover:bg-primary/10 " onClick={() => navigate("/hr/employees")}>
              <Users className="h-4 w-4" />
              دليل الموظفين
            </Button>
          </>
        }
      />

      {/* Stats Grid */}
      <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
        <StatCard
          title="إجمالي القوة"
          value={formatNumber(stats?.totalEmployees || 0)}
          icon={<Users className="h-5 w-5" />}
          description="موظف مسجل حالياً"
          isLoading={loadingStats}
          variant="blue"
        />
        <StatCard
          title="طلبات الإجازة"
          value={formatNumber(stats?.pendingLeaveRequests || 0)}
          icon={<CalendarDays className="h-5 w-5" />}
          description="طلبات معلقة قيد الانتظار"
          isLoading={loadingStats}
          variant="orange"
        />
        <StatCard
          title="موظفين في إجازة"
          value={formatNumber(stats?.employeesOnLeave || 0)}
          icon={<Clock className="h-5 w-5" />}
          description="موظفون خارج الخدمة حالياً"
          isLoading={loadingStats}
          variant="rose"
        />
      </div>

      {/* Main Content Layout */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">

        {/* Recent Activity: Leaves */}
        <Card className="lg:col-span-2 p-6 border-border bg-card border-2">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-orange-500/10 rounded-xl">
                <Clock className="h-5 w-5 text-orange-500" />
              </div>
               <Heading level="h3" className="font-black text-lg">أحدث طلبات الإجازة</Heading>
            </div>
            <Button variant="ghost" size="sm" className="text-primary hover:text-primary/10 gap-1 text-xs font-bold" onClick={() => navigate("/hr/leaves")}>
              عرض الكل
              <ArrowUpRight className="h-3 w-3" />
            </Button>
          </div>

          <div className="space-y-3">
            {loadingLeaves ? (
              Array(3).fill(0).map((_, i) => <Skeleton key={i} className="h-16 w-full rounded-xl" />)
            ) : leaves?.items.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 text-muted-foreground bg-muted/5 rounded-2xl border-2 border-dashed">
                <CalendarDays className="h-10 w-10 opacity-10 mb-2" />
                <p className="text-sm italic font-medium">لا توجد طلبات إجازة حالياً</p>
              </div>
            ) : (
              leaves?.items.map((leave: LeaveRequestListItemDto) => (
                <div key={leave.leaveRequestID} className="flex items-center justify-between p-4 rounded-xl bg-background border border-border hover:border-primary/30 transition-all group ">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary font-black text-xs group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                      {leave.employeeFullName[0]}
                    </div>
                    <div>
                      <p className="text-sm font-black text-foreground">{leave.employeeFullName}</p>
                      <p className="text-[10px] sm:text-xs text-muted-foreground font-bold">{leave.leaveTypeName} · {leave.requestedDays} أيام</p>
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    <Badge variant={leave.status === LEAVE_STATUS.APPROVED ? 'success' : leave.status === LEAVE_STATUS.PENDING ? 'warning' : 'danger'} className="text-[9px] font-black h-5">
                      {leave.status === LEAVE_STATUS.APPROVED ? 'معتمد' : leave.status === LEAVE_STATUS.PENDING ? 'قيد الانتظار' : 'مرفوض'}
                    </Badge>
                    <span className="text-[9px] text-muted-foreground font-mono">{formatDate(leave.startDate)}</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </Card>

        {/* Action Center / Quick Tasks */}
        <Card className="p-6 border-border bg-card border-2 relative overflow-hidden">
          <div className="relative z-10 flex flex-col h-full">
            <div className="flex items-center gap-3 mb-6">
              <div className="p-2 bg-emerald-500/10 rounded-xl ">
                <CheckCircle2 className="h-5 w-5 text-emerald-500" />
              </div>
               <Heading level="h3" className="font-black text-lg">المهام السريعة</Heading>
            </div>

            <div className="space-y-4 flex-1">
              <QuickTask
                title="إضافة موظف"
                desc="تسجيل منتسب جديد في النظام"
                icon={<Plus className="h-4 w-4" />}
                onClick={() => setIsCreateModalOpen(true)}
              />
              <QuickTask
                title="تقارير الأداء"
                desc="استخراج إحصائيات القوة"
                icon={<ArrowUpRight className="h-4 w-4" />}
                onClick={() => navigate("/reports")}
              />
            </div>

            <div className="mt-8 p-4 rounded-2xl bg-primary/5 border border-primary/10 border-dashed">
              <p className="text-[10px] sm:text-xs font-black text-primary uppercase mb-1 flex items-center gap-1.5">
                <span className="w-1 h-1 rounded-full bg-primary inline-block" /> نصيحة تقنية
              </p>
              <p className="text-xs text-muted-foreground leading-relaxed font-medium">
                يمكنك استخدام سجل الخدمة الآلي لتتبع مسيرة الموظف المهنية والمالية بدقة عالية دون تدخل يدوي.
              </p>
            </div>
          </div>
          <div className="absolute -bottom-12 -start-12 w-48 h-48 bg-primary/5 rounded-full blur-[80px] pointer-events-none" />
        </Card>
      </div>

      <CreateEmployeeDialog
        open={isCreateModalOpen}
        onOpenChange={(o) => !o && setIsCreateModalOpen(false)}
      />
    </PageLayout>
  );
}

function QuickTask({ title, desc, icon, onClick }: { title: string, desc: string, icon: React.ReactNode, onClick: () => void }) {
  return (
    <Button
      variant="ghost"
      onClick={onClick}
      className="w-full p-4 rounded-2xl bg-background border border-border flex items-center gap-4 hover:bg-primary/[0.02] hover:border-primary/30 text-start group "
    >
      <div className="w-10 h-10 rounded-xl bg-muted/50 flex items-center justify-center text-muted-foreground group-hover:text-primary group-hover:bg-primary/10 transition-all  border border-transparent group-hover:border-primary/10">
        {icon}
      </div>
      <div>
        <p className="text-sm font-black text-foreground group-hover:text-primary transition-colors">{title}</p>
        <p className="text-[10px] sm:text-xs text-muted-foreground font-medium">{desc}</p>
      </div>
    </Button>
  )
}
