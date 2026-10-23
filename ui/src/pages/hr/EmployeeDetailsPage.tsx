import { useQuery } from "@tanstack/react-query";
import {
  BadgeInfo,
  Briefcase,
  CalendarDays,
  ClipboardList,
  Clock,
  Download,
  Edit,
  Eye,
  FileText,
  History,
  Phone,
  Plus,
  Printer,
  ShieldCheck,
  Trash2,
  Upload
} from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate,useParams } from "react-router-dom";
import { toast } from "sonner";

import type { HrDocumentDto, ServiceHistoryEventDto } from "@/api/hr";
import { hrApi } from "@/api/hr";
import { AttendanceDialog } from "@/components/domain/hr/AttendanceDialog";
import { CreateEmployeeDialog } from "@/components/domain/hr/CreateEmployeeDialog";
import { HrUploadDocumentDialog } from "@/components/domain/hr/HrUploadDocumentDialog";
import { ServiceHistoryDialog } from "@/components/domain/hr/ServiceHistoryDialog";
import { type DocumentItem,DocumentViewerDialog } from "@/components/domain/retirement/DocumentViewerDialog";
import { DetailLayout } from "@/components/layout/DetailLayout";
import {
  Badge,
  Breadcrumb,
  Button,
  Card,
  type CustomColumnDef,
  DataTable,
  EditableImage,
  ErrorState,
  Heading,
  InfoPair,
  PageLayout,
  Skeleton,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
  useConfirm} from "@/components/ui";
import { LEAVE_STATUS } from "@/constants";
import { GENDER } from "@/constants";
import { useApiMutation } from "@/hooks";
import { getDocumentUrl } from "@/lib/utils";
import { formatDate, getPhotoUrl } from "@/lib/utils";

// ─── Helper for Image URLs ────────────────────────────

export function EmployeeDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const employeeId = parseInt(id || "0");
  
  const [editDialogOpen, setEditDialogOpen] = useState(false);

  const { data: employee, isLoading, isError, error, refetch } = useQuery({
    queryKey: ["hr", "employee", employeeId],
    queryFn: () => hrApi.getEmployeeById(employeeId),
    enabled: !!employeeId
  });

  useEffect(() => {
    if (error) toast.error("تعذر تحميل بيانات الموظف", { id: "employee-details" });
  }, [error]);

  if (isLoading) return <EmployeeDetailsSkeleton />;
  if (isError || !employee) {
    return (
      <PageLayout>
        <ErrorState message="الموظف غير موجود" />
      </PageLayout>
    );
  }

  // ─── Robust Value Extraction ──────────────────────────
  // Handle variations in JSON property naming (NationalID, nationalID, nationalId)
  const getVal = (obj: Record<string, unknown>, camel: string, pascal: string, lower?: string): string | null => {
    const val = (obj[camel] ?? obj[pascal] ?? (lower ? obj[lower] : null)) as string | number | null;
    return val !== undefined && val !== null ? String(val) : null;
  };

  const employeeData = employee as unknown as Record<string, unknown>;
  const nationalID = getVal(employeeData, "nationalID", "NationalID", "nationalId");
  const militaryNumber = getVal(employeeData, "militaryNumber", "MilitaryNumber");
  const rankName = getVal(employeeData, "rankName", "RankName");
  const phone = getVal(employeeData, "phone", "Phone");
  const departmentName = getVal(employeeData, "departmentName", "DepartmentName") || "غير محدد";
  const jobTitleName = getVal(employeeData, "jobTitleName", "JobTitleName");

  return (
    <PageLayout>
    <DetailLayout
      breadcrumb={
        <Breadcrumb
          items={[
            { label: "الموارد البشرية", href: "/hr" },
            { label: "شؤون الموظفين", href: "/hr/employees" },
            { label: employee.fullName }
          ]}
        />
      }
      headerTitle={
        <div className="flex flex-col text-start">
          <Heading level="h1" className="font-black">{employee.fullName}</Heading>
          <p className="text-muted-foreground font-medium">الرقم الوظيفي: {employee.employeeNumber}</p>
        </div>
      }
      headerActions={
        <div className="flex gap-2 flex-wrap">
          <Button variant="outline" className="gap-2" onClick={() => navigate(`/hr/employees/print/${employeeId}`)}>
            <Printer className="h-4 w-4" />
            طباعة الاستمارة
          </Button>
          <Button className="gap-2" onClick={() => setEditDialogOpen(true)}>
            <Edit className="h-4 w-4" />
            تعديل البيانات
          </Button>
        </div>
      }
      sidebar={
        <div className="space-y-6">
          <Card className="p-6 space-y-6">
            <div className="flex flex-col items-center text-center space-y-4">
              <EditableImage 
                src={getPhotoUrl(employee.profilePhotoPath)} 
                alt={employee.fullName}
                onUpload={async (file) => { await hrApi.uploadEmployeePhoto(employeeId, file); }}
                className="w-32 h-32 rounded-2xl border-4 border-card"
              />
              <div>
                <Heading level="h2" className="font-black">{employee.fullName}</Heading>
                <Badge variant="secondary" className="mt-1">{jobTitleName}</Badge>
              </div>
            </div>

            <div className="space-y-4 pt-4 border-t">
              <InfoPair label="الرتبة" value={rankName || "—"} icon={<ShieldCheck className="h-4 w-4" />} layout="row" />
              <InfoPair label="الإدارة" value={departmentName} icon={<Briefcase className="h-4 w-4" />} layout="row" />
              <InfoPair label="رقم الهاتف" value={phone || "—"} icon={<Phone className="h-4 w-4" />} layout="row" />
            </div>
          </Card>

          <Card className="p-5 space-y-4">
            <div className="flex items-center gap-2 text-primary border-b pb-2 mb-2">
                <Clock className="h-4 w-4" />
                <Heading level="h3" className="font-bold text-xs uppercase">معلومات النظام</Heading>
            </div>
            <div className="space-y-3">
                <InfoPair label="تاريخ الإضافة" value={formatDate(employee.createdAt)} layout="row" />
                <InfoPair label="آخر تحديث" value={formatDate(employee.updatedAt) || "—"} layout="row" />
                <div className="flex justify-between items-center text-sm border-b border-muted pb-2 last:border-0 last:pb-0">
                    <span className="text-muted-foreground">حالة الخدمة</span>
                    <Badge variant={employee.isInService ? "success" : "warning"}>
                        {employee.isInService ? "في الخدمة" : "خارج الخدمة"}
                    </Badge>
                </div>
            </div>
          </Card>
        </div>
      }
    >
      <div className="space-y-6" dir="rtl">
        {/* Main Tabs Section */}
        <Tabs defaultValue="attendance" className="w-full">
          <TabsList className="grid w-full grid-cols-2 sm:grid-cols-4 bg-muted/50 p-1 min-h-[48px] sm:h-12 rounded-xl">
            <TabsTrigger value="attendance" className="rounded-lg gap-2 font-bold data-[state=active]:bg-background ">
              <ClipboardList className="h-4 w-4" />
              سجل الحضور
            </TabsTrigger>
            <TabsTrigger value="leaves" className="rounded-lg gap-2 font-bold data-[state=active]:bg-background ">
              <CalendarDays className="h-4 w-4" />
              طلبات الإجازة
            </TabsTrigger>
            <TabsTrigger value="history" className="rounded-lg gap-2 font-bold data-[state=active]:bg-background ">
              <History className="h-4 w-4" />
              السجل الوظيفي
            </TabsTrigger>
            <TabsTrigger value="documents" className="rounded-lg gap-2 font-bold data-[state=active]:bg-background ">
              <FileText className="h-4 w-4" />
              الوثائق
            </TabsTrigger>
          </TabsList>

          <div className="mt-6">
            <TabsContent value="attendance" className="mt-0">
              <EmployeeAttendanceTab employeeId={employeeId} />
            </TabsContent>

            <TabsContent value="leaves" className="mt-0">
              <EmployeeLeavesTab employeeId={employeeId} />
            </TabsContent>

            <TabsContent value="history" className="mt-0">
              <EmployeeHistoryTab employeeId={employeeId} />
            </TabsContent>

            <TabsContent value="documents" className="mt-0">
              <EmployeeDocumentsTab employeeId={employeeId} />
            </TabsContent>
          </div>
        </Tabs>

        {/* Basic Info Details */}
        <Card className="p-6">
          <Heading level="h3" className="text-lg font-bold flex items-center gap-2 mb-6 text-start">
            <BadgeInfo className="h-5 w-5 text-primary" />
            البيانات الأساسية والتكميلية
          </Heading>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-y-6 text-start">
            <InfoPair label="الرقم الوطني" value={nationalID || "—"} mono />
            <InfoPair label="الجنس" value={employee.gender === GENDER.MALE ? "ذكر" : "أنثى"} />
            <InfoPair label="تاريخ الميلاد" value={formatDate(employee.birthDate) || "—"} />
            <InfoPair label="مكان الميلاد" value={employee.birthPlace || "—"} />
            <InfoPair label="الحالة الاجتماعية" value={employee.maritalStatus || "—"} />
            <InfoPair label="فصيلة الدم" value={employee.bloodGroup || "—"} />
            <InfoPair label="تاريخ التعيين" value={formatDate(employee.hireDate) || "—"} />
            <InfoPair label="الرقم العسكري" value={militaryNumber || "—"} mono />
            <InfoPair label="تاريخ الخدمة العسكرية" value={formatDate(employee.militaryServiceDate) || "—"} />
          </div>
          
          <div className="mt-8 pt-6 border-t grid grid-cols-1 md:grid-cols-2 gap-6 text-start">
            <InfoPair label="العنوان" value={employee.address || "—"} />
            <InfoPair label="جهة اتصال الطوارئ" value={employee.emergencyContactName ? `${employee.emergencyContactName} (${employee.emergencyContactPhone})` : "—"} />
          </div>
        </Card>
      </div>

      <CreateEmployeeDialog 
        open={editDialogOpen}
        onOpenChange={setEditDialogOpen}
        employeeId={employeeId}
        onSaved={() => refetch()}
      />
    </DetailLayout>
    </PageLayout>
  );
}

// ─── Sub-Tabs ──────────────────────────────────────────

function EmployeeAttendanceTab({ employeeId }: { employeeId: number }) {
  const [open, setOpen] = useState(false);
  const { data: attendanceRes, isLoading } = useQuery({
    queryKey: ["hr", "attendance", employeeId],
    queryFn: () => hrApi.getAttendance({ employeeId, pageSize: 50 })
  });

  const events = attendanceRes?.items || [];

  return (
    <Card className="p-6">
      <div className="flex justify-between items-center mb-6 text-start">
        <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
          <ClipboardList className="h-5 w-5 text-primary" />
          سجل الحضور والانصراف
        </Heading>
        <Button size="sm" className="gap-2" variant="outline" onClick={() => setOpen(true)}>
          <Plus className="h-4 w-4" />
          تسجيل يدوي
        </Button>
      </div>

      <DataTable
        columns={[
          { key: "eventDate", header: "التاريخ", render: (row) => formatDate(row.eventDate) },
          { key: "eventTime", header: "الوقت", accessor: (row) => row.eventTime || "—" },
          { 
            key: "eventType", 
            header: "النوع", 
            render: (row) => (
              <Badge variant={row.eventType === "CheckIn" ? "success" : "warning"}>
                {row.eventType === "CheckIn" ? "دخول" : "خروج"}
              </Badge>
            )
          },
          { key: "location", header: "الموقع", accessor: (row) => row.location || "—" },
          { key: "isManualEntry", header: "المصدر", render: (row) => row.isManualEntry ? "يدوي" : "جهاز البصمة" },
        ]}
        data={events}
        loading={isLoading}
      />

      <AttendanceDialog 
        open={open}
        onOpenChange={setOpen}
        employeeId={employeeId}
      />
    </Card>
  );
}

function EmployeeLeavesTab({ employeeId }: { employeeId: number }) {
  const { data: leavesRes, isLoading } = useQuery({
    queryKey: ["hr", "leaves", employeeId],
    queryFn: () => hrApi.getLeaveRequests({ employeeId })
  });

  const leaves = leavesRes?.items || [];

  return (
    <Card className="p-6">
      <div className="flex justify-between items-center mb-6 text-start">
        <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
          <CalendarDays className="h-5 w-5 text-primary" />
          تاريخ الإجازات
        </Heading>
        <Button size="sm" className="gap-2" variant="outline">
          <Plus className="h-4 w-4" />
          طلب إجازة
        </Button>
      </div>

      <DataTable
        columns={[
          { key: "leaveTypeName", header: "نوع الإجازة" },
          { key: "startDate", header: "من تاريخ", render: (row) => formatDate(row.startDate) },
          { key: "endDate", header: "إلى تاريخ", render: (row) => formatDate(row.endDate) },
          { key: "requestedDays", header: "المدة (يوم)", accessor: (row) => row.requestedDays },
          { 
            key: "status", 
            header: "الحالة",
            render: (row) => (
              <Badge variant={row.status === LEAVE_STATUS.APPROVED ? "success" : row.status === LEAVE_STATUS.REJECTED ? "danger" : "warning"}>
                {row.status === LEAVE_STATUS.APPROVED ? "مقبولة" : row.status === LEAVE_STATUS.REJECTED ? "مرفوضة" : "قيد المراجعة"}
              </Badge>
            )
          },
        ]}
        data={leaves}
        loading={isLoading}
      />
    </Card>
  );
}

function EmployeeHistoryTab({ employeeId }: { employeeId: number }) {
  const [addDialogOpen, setAddDialogOpen] = useState(false);
  const [selectedEvent, setSelectedEvent] = useState<ServiceHistoryEventDto | null>(null);
  
  const { data: history = [], isLoading, refetch } = useQuery({
    queryKey: ["hr", "history", employeeId],
    queryFn: () => hrApi.getEmployeeServiceHistory(employeeId)
  });

  const deleteMutation = useApiMutation(
    (id: number) => hrApi.deleteHistoryEvent(id),
    {
      successMessage: "تم حذف قيد السجل بنجاح",
      invalidateKeys: [["hr", "history", employeeId]]
    }
  );

  const { confirm, ConfirmDialogElement } = useConfirm();

  const handleEdit = (event: ServiceHistoryEventDto) => {
    setSelectedEvent(event);
    setAddDialogOpen(true);
  };

  const handleDelete = async (event: ServiceHistoryEventDto) => {
    const ok = await confirm({
      title: "حذف قيد من السجل",
      description: `هل أنت متأكد من حذف قيد "${event.eventType}"؟ هذا الإجراء لا يمكن التراجع عنه.`,
      variant: "danger"
    });
    if (ok) deleteMutation.mutate(event.serviceHistoryEventID);
  };

  const handleAdd = () => {
    setSelectedEvent(null);
    setAddDialogOpen(true);
  };

  return (
    <Card className="p-6">
      <div className="flex justify-between items-center mb-6 text-start">
        <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
          <History className="h-5 w-5 text-primary" />
          السجل الوظيفي والترقيات
        </Heading>
        <Button size="sm" className="gap-2" onClick={handleAdd}>
          <Plus className="h-4 w-4" />
          إضافة قيد جديد
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map(i => <Skeleton key={i} className="h-20 w-full rounded-xl" />)}
        </div>
      ) : history.length === 0 ? (
        <div className="py-12 text-center text-muted-foreground italic">لا يوجد سجل تاريخي متاح</div>
      ) : (
        <div className="space-y-4">
          {history.map((item: ServiceHistoryEventDto, idx: number) => (
            <div key={idx} className="group flex flex-wrap gap-2 p-4 rounded-xl border border-border hover:bg-muted/30 transition-colors">
              <div className="w-10 h-10 rounded-full bg-muted flex items-center justify-center text-muted-foreground shrink-0 mt-1">
                <Clock className="w-5 h-5" />
              </div>
              <div className="flex-1 space-y-1 text-start">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                        <Badge variant="outline" className="text-[10px] sm:text-xs uppercase font-bold">{item.eventType}</Badge>
                        <span className="text-xs text-muted-foreground font-mono">{formatDate(item.eventDate)}</span>
                    </div>
                    <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-blue-600" onClick={() => handleEdit(item)}>
                            <Edit className="h-3.5 w-3.5" />
                        </Button>
                        <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-destructive" onClick={() => handleDelete(item)}>
                            <Trash2 className="h-3.5 w-3.5" />
                        </Button>
                    </div>
                </div>
                <p className="font-bold text-sm leading-relaxed">{item.summary}</p>
              </div>
            </div>
          ))}
        </div>
      )}

      <ServiceHistoryDialog 
        open={addDialogOpen}
        onOpenChange={(o) => {
            setAddDialogOpen(o);
            if (!o) setSelectedEvent(null);
        }}
        employeeId={employeeId}
        event={selectedEvent}
        onSuccess={() => refetch()}
      />
      {ConfirmDialogElement}
    </Card>
  );
}

function EmployeeDocumentsTab({ employeeId }: { employeeId: number }) {
  const [viewerOpen, setViewerOpen] = useState(false);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [selectedDoc, setSelectedDoc] = useState<DocumentItem | null>(null);

  const { data: docs = [], isLoading } = useQuery({
    queryKey: ["hr", "documents", employeeId],
    queryFn: () => hrApi.getEmployeeDocuments(employeeId)
  });

  const deleteMutation = useApiMutation(
    (docId: number) => hrApi.deleteDocument(docId),
    {
      successMessage: "تم حذف الوثيقة بنجاح",
      invalidateKeys: [["hr", "documents", employeeId]]
    }
  );

  const { confirm, ConfirmDialogElement } = useConfirm();

  const handleDelete = async (doc: HrDocumentDto) => {
    const ok = await confirm({
      title: "حذف الوثيقة",
      description: `هل أنت متأكد من حذف الوثيقة "${doc.fileName}"؟`,
      variant: "danger"
    });
    if (ok) deleteMutation.mutate(doc.documentID);
  };

  const handleView = (doc: HrDocumentDto) => {
    setSelectedDoc({
      documentID: doc.documentID,
      fileName: doc.fileName,
      filePath: doc.filePath,
      documentTypeName: doc.documentTypeName,
      documentTypeID: 0,
      createdAt: doc.createdAt
    });
    setViewerOpen(true);
  };

  const columns: CustomColumnDef<HrDocumentDto>[] = [
    { key: "documentTypeName", header: "نوع الوثيقة" },
    { key: "fileName", header: "اسم الملف", accessor: (row) => row.fileName },
    {
      key: "createdAt",
      header: "تاريخ الرفع",
      render: (row) => <span className="font-mono text-xs">{formatDate(row.createdAt)}</span>
    },
    {
      key: "actions",
      header: "إجراءات",
      render: (row) => (
        <div className="flex gap-1">
                          <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-blue-600" onClick={() => handleView(row)}>
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-primary" onClick={() => window.open(getDocumentUrl(row.filePath) ?? "", '_blank')}>
                            <Download className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" className="min-h-[44px] min-w-[44px] text-destructive" onClick={() => handleDelete(row)}>
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      )
    }
  ];

  return (
    <Card className="p-6">
      <div className="flex justify-between items-center mb-6 text-start">
        <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
          <FileText className="h-5 w-5 text-primary" />
          الوثائق والمرفقات
        </Heading>
        <Button size="sm" className="gap-2" variant="outline" onClick={() => setUploadOpen(true)}>
          <Upload className="h-4 w-4" />
          رفع وثيقة
        </Button>
      </div>

      <DataTable
        columns={columns}
        data={docs}
        loading={isLoading}
        emptyMessage="لا توجد وثائق مرفوعة حالياً"
      />

      {selectedDoc && (
        <DocumentViewerDialog
          open={viewerOpen}
          onOpenChange={setViewerOpen}
          document={selectedDoc}
        />
      )}

      <HrUploadDocumentDialog
        open={uploadOpen}
        onOpenChange={setUploadOpen}
        employeeId={employeeId}
      />

      {ConfirmDialogElement}
    </Card>
  );
}

// ─── Skeletons ─────────────────────────────────────────

function EmployeeDetailsSkeleton() {
  return (
    <PageLayout>
      <div className="flex items-center gap-4 text-start">
        <Skeleton className="h-10 w-10 rounded-full" />
        <div className="space-y-2">
          <Skeleton className="h-8 w-full max-w-[200px]" />
          <Skeleton className="h-4 w-full max-w-[160px]" />
        </div>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Skeleton className="h-48 sm:h-64 w-full rounded-2xl" />
        <Skeleton className="h-48 sm:h-64 w-full rounded-2xl" />
      </div>
    </PageLayout>
  );
}
