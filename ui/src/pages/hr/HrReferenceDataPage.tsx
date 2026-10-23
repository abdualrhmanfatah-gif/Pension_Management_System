import { useQuery } from "@tanstack/react-query";
import {
  Activity,
  Briefcase,
  Building2,
  Edit,
  Layers} from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { toast } from "sonner";

import type { DepartmentDto, EmploymentStatusDto,JobTitleDto, SectionDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { DepartmentDialog } from "@/components/domain/hr/DepartmentDialog";
import { JobTitleDialog } from "@/components/domain/hr/JobTitleDialog";
import { SectionDialog } from "@/components/domain/hr/SectionDialog";
import {
  Badge,
  Button,
  CrudPageLayout,
  type CustomColumnDef,
  ErrorState,
  PageLayout,
  RecordStateBadge,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger} from "@/components/ui";

/**
 * صفحة البيانات المرجعية والهيكل التنظيمي - وحدة الموارد البشرية
 */
export function HrReferenceDataPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [activeTab, setActiveTab] = useState(searchParams.get("tab") || "departments");

  // حوارات الإدارة
  const [isDeptModalOpen, setIsDeptModalOpen] = useState(false);
  const [selectedDept, setSelectedDept] = useState<DepartmentDto | null>(null);

  // حوارات الأقسام
  const [isSecModalOpen, setIsSecModalOpen] = useState(false);
  const [selectedSec, setSelectedSec] = useState<SectionDto | null>(null);

  // حوارات المسميات الوظيفية
  const [isJobModalOpen, setIsJobModalOpen] = useState(false);
  const [selectedJob, setSelectedJob] = useState<JobTitleDto | null>(null);

  return (
    <PageLayout>
      {/* Header */}


      <Tabs value={activeTab} onValueChange={(v) => { setActiveTab(v); setSearchParams((p) => { p.set("tab", v); return p; }, { replace: true }); }} className="space-y-6">
        <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
          <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
            <TabsTrigger value="departments" className="gap-2 px-6 py-2.5">
              <Building2 className="h-4 w-4" /> الإدارات
            </TabsTrigger>
            <TabsTrigger value="sections" className="gap-2 px-6 py-2.5">
              <Layers className="h-4 w-4" /> الأقسام
            </TabsTrigger>
            <TabsTrigger value="job-titles" className="gap-2 px-6 py-2.5">
              <Briefcase className="h-4 w-4" /> المسميات الوظيفية
            </TabsTrigger>
            <TabsTrigger value="status" className="gap-2 px-6 py-2.5">
              <Activity className="h-4 w-4" /> حالات الخدمة
            </TabsTrigger>
          </TabsList>
        </div>

        <TabsContent value="departments" className="animate-in fade-in duration-300 outline-none">
          <DepartmentsTab
            onEdit={(dept) => {
              setSelectedDept(dept);
              setIsDeptModalOpen(true);
            }}
            onAdd={() => {
              setSelectedDept(null);
              setIsDeptModalOpen(true);
            }}
          />
        </TabsContent>

        <TabsContent value="sections" className="animate-in fade-in duration-300 outline-none">
          <SectionsTab
            onEdit={(sec) => {
              setSelectedSec(sec);
              setIsSecModalOpen(true);
            }}
            onAdd={() => {
              setSelectedSec(null);
              setIsSecModalOpen(true);
            }}
          />
        </TabsContent>

        <TabsContent value="job-titles" className="animate-in fade-in duration-300 outline-none">
          <JobTitlesTab
            onEdit={(job) => {
              setSelectedJob(job);
              setIsJobModalOpen(true);
            }}
            onAdd={() => {
              setSelectedJob(null);
              setIsJobModalOpen(true);
            }}
          />
        </TabsContent>

        <TabsContent value="status" className="animate-in fade-in duration-300 outline-none">
          <EmploymentStatusesTab />
        </TabsContent>
      </Tabs>

      <DepartmentDialog
        open={isDeptModalOpen}
        onClose={() => setIsDeptModalOpen(false)}
        initialData={selectedDept}
      />

      <SectionDialog
        open={isSecModalOpen}
        onClose={() => setIsSecModalOpen(false)}
        initialData={selectedSec}
      />

      <JobTitleDialog
        open={isJobModalOpen}
        onClose={() => setIsJobModalOpen(false)}
        initialData={selectedJob}
    />
  </PageLayout>
  );
}

// ─── Sub-Tabs (Internal Components) ────────────────────

function DepartmentsTab({ onEdit, onAdd }: { onEdit: (d: DepartmentDto) => void, onAdd: () => void }) {
  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "departments"],
    queryFn: () => hrApi.getDepartments()
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل البيانات المرجعية", { id: "hr-reference-data" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل البيانات المرجعية" />;
  }

  const columns = useMemo<CustomColumnDef<DepartmentDto>[]>(() => [
    {
      key: "code",
      header: "الكود",
      render: (r) => <span className="font-mono text-xs font-black text-primary bg-primary/5 px-2 py-1 rounded-md border border-primary">{r.code}</span>
    },
    { key: "name", header: "اسم الإدارة", render: (r) => <span className="font-bold text-foreground">{r.name}</span> },
    { key: "employeesCount", header: "عدد الموظفين", render: (r) => <span className="font-mono font-bold">{r.employeesCount}</span> },
    { key: "sectionsCount", header: "الأقسام", render: (r) => <span className="font-mono font-bold text-muted-foreground">{r.sectionsCount}</span> },
    { key: "isActive", header: "الحالة", render: (r) => <RecordStateBadge state={r.isActive ? "approved" : "suspended"} /> },
    {
      key: "actions",
      header: "إجراءات",
      render: (r) => (
        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-blue-600 hover:bg-blue-50" onClick={() => onEdit(r)}>
          <Edit className="h-4 w-4" />
        </Button>
      )
    }
  ], [onEdit]);

  return (
    <CrudPageLayout
      title="الإدارات المعتمدة"
      description="إدارة الوحدات التنظيمية الرئيسية في هيكل الصندوق"
      onCreateClick={onAdd}
      createLabel="إضافة إدارة"
      columns={columns}
      data={data || []}
      loading={isLoading}
    />
  );
}

function SectionsTab({ onEdit, onAdd }: { onEdit: (s: SectionDto) => void, onAdd: () => void }) {
  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "sections"],
    queryFn: () => hrApi.getSections()
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل البيانات المرجعية", { id: "hr-reference-data" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل البيانات المرجعية" />;
  }

  const columns = useMemo<CustomColumnDef<SectionDto>[]>(() => [
    {
      key: "code",
      header: "الكود",
      render: (r) => <span className="font-mono text-xs font-black text-primary bg-primary/5 px-2 py-1 rounded-md border border-primary">{r.code}</span>
    },
    { key: "name", header: "اسم القسم", render: (r) => <span className="font-bold text-foreground">{r.name}</span> },
    { key: "departmentName", header: "الإدارة التابع لها", render: (r) => <span className="text-xs font-medium text-muted-foreground">{r.departmentName}</span> },
    { key: "employeesCount", header: "الموظفين", render: (r) => <span className="font-mono font-bold">{r.employeesCount}</span> },
    { key: "isActive", header: "الحالة", render: (r) => <RecordStateBadge state={r.isActive ? "approved" : "suspended"} /> },
    {
      key: "actions",
      header: "إجراءات",
      render: (r) => (
        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-blue-600 hover:bg-blue-50" onClick={() => onEdit(r)}>
          <Edit className="h-4 w-4" />
        </Button>
      )
    }
  ], [onEdit]);

  return (
    <CrudPageLayout
      title="الأقسام التنظيمية"
      description="إدارة الأقسام والوحدات التابعة للإدارات العامة"
      onCreateClick={onAdd}
      createLabel="إضافة قسم"
      columns={columns}
      data={data || []}
      loading={isLoading}
    />
  );
}

function JobTitlesTab({ onEdit, onAdd }: { onEdit: (j: JobTitleDto) => void, onAdd: () => void }) {
  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "jobTitles"],
    queryFn: () => hrApi.getJobTitles()
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل البيانات المرجعية", { id: "hr-reference-data" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل البيانات المرجعية" />;
  }

  const columns = useMemo<CustomColumnDef<JobTitleDto>[]>(() => [
    {
      key: "code",
      header: "الكود",
      render: (r) => <span className="font-mono text-xs font-black text-primary bg-primary/5 px-2 py-1 rounded-md border border-primary">{r.code}</span>
    },
    { key: "name", header: "المسمى الوظيفي", render: (r) => <span className="font-bold text-foreground">{r.name}</span> },
    { key: "employeesCount", header: "عدد الموظفين", render: (r) => <span className="font-mono font-bold">{r.employeesCount}</span> },
    { key: "isActive", header: "الحالة", render: (r) => <RecordStateBadge state={r.isActive ? "approved" : "suspended"} /> },
    {
      key: "actions",
      header: "إجراءات",
      render: (r) => (
        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-blue-600 hover:bg-blue-50" onClick={() => onEdit(r)}>
          <Edit className="h-4 w-4" />
        </Button>
      )
    }
  ], [onEdit]);

  return (
    <CrudPageLayout
      title="دليل المسميات الوظيفية"
      description="إدارة قائمة المسميات والوظائف المعتمدة ضمن الكادر الوظيفي"
      onCreateClick={onAdd}
      createLabel="إضافة مسمى"
      columns={columns}
      data={data || []}
      loading={isLoading}
    />
  );
}

function EmploymentStatusesTab() {
  const { data, isLoading, error } = useQuery({
    queryKey: ["hr", "employmentStatuses"],
    queryFn: () => hrApi.getEmploymentStatuses()
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل البيانات المرجعية", { id: "hr-reference-data" });
  }, [error]);

  if (error) {
    return <ErrorState message="تعذر تحميل البيانات المرجعية" />;
  }

  const columns = useMemo<CustomColumnDef<EmploymentStatusDto>[]>(() => [
    { key: "code", header: "الكود", render: (r) => <span className="font-mono text-xs font-bold">{r.code}</span> },
    { key: "name", header: "الحالة الوظيفية", render: (r) => <span className="font-bold">{r.name}</span> },
    {
      key: "isInService", header: "حالة الخدمة", render: (r) => (
        <Badge variant={r.isInService ? "success" : "warning"} className="font-black">
          {r.isInService ? "داخل الخدمة" : "خارج الخدمة"}
        </Badge>
      )
    },
    { key: "employeesCount", header: "عدد الموظفين", render: (r) => <span className="font-mono font-bold">{r.employeesCount}</span> }
  ], []);

  return (
    <CrudPageLayout
      title="حالات الخدمة"
      description="تصنيفات حالات العمل والدوام الرسمية المعتمدة في النظام"
      columns={columns}
      data={data || []}
      loading={isLoading}
    />
  );
}
