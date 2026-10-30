import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ArrowRight,
  Building2,
  CalendarDays,
  Clock,
  Download,
  Edit,
  FileText,
  Hash,
  Info,
  Trash2,
  User} from "lucide-react";
import { useCallback } from "react";
import { useNavigate,useParams } from "react-router-dom";

import { correspondenceApi } from "@/api/correspondence";
import type { HrDocumentDto } from "@/api/hr";
<<<<<<< HEAD
import { CorrespondenceFormDialog } from "@/components/domain/correspondence/CorrespondenceFormDialog";
=======
import { useHasPermission } from "@/stores/authStore";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { DetailLayout } from "@/components/layout/DetailLayout";
import {
  Badge,
  Breadcrumb,
  Button,
  Card,
  type CustomColumnDef,
  DataTable,
  FileUploader,
  Heading,
  InfoPair,
  Skeleton,
  Text,
  useConfirm} from "@/components/ui";
<<<<<<< HEAD
import { useApiMutation, useCrudState } from "@/hooks";
import { formatDate, getDocumentUrl } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
=======
import { CorrespondenceFormDialog } from "@/components/domain/correspondence/CorrespondenceFormDialog";
import { useApiMutation, useCrudState } from "@/hooks";
import { formatDate, getDocumentUrl } from "@/lib/utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  type CorrespondenceDetailsDto,
  CorrespondenceDirection} from "@/types/correspondence";

export function CorrespondenceDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { confirm, ConfirmDialogElement } = useConfirm();
  const canManage = useHasPermission("System.Admin");
  const canUpload = useHasPermission("File.Documents.Upload", "System.Admin");

  const correspondenceId = Number(id);

  const { data: res, isLoading } = useQuery({
    queryKey: ["correspondence", "detail", correspondenceId],
    queryFn: () => correspondenceApi.getById(correspondenceId),
    enabled: !!correspondenceId
  });

  const { data: docsRes, isLoading: loadingDocs } = useQuery({
    queryKey: ["correspondence", "documents", correspondenceId],
    queryFn: () => correspondenceApi.getDocuments(correspondenceId),
    enabled: !!correspondenceId
  });

  const crud = useCrudState<CorrespondenceDetailsDto>();
  const correspondence = res?.data;
  const documents = docsRes?.data || [];

  const deleteMutation = useApiMutation(
    () => correspondenceApi.delete(correspondenceId),
    {
      successMessage: "تم حذف القيد بنجاح",
      onSuccess: () => {
        const backPath = correspondence?.direction === CorrespondenceDirection.Incoming
          ? "/correspondence/incoming"
          : "/correspondence/outgoing";
        navigate(backPath);
      }
    }
  );

  const handleDelete = async () => {
    const ok = await confirm({
      title: "حذف القيد",
      description: "هل أنت متأكد من حذف هذا القيد نهائياً؟ لا يمكن التراجع عن هذا الإجراء.",
      variant: "danger"
    });
    if (ok) deleteMutation.mutate(undefined);
  };

  const uploadMutation = useApiMutation(
    (file: File) => correspondenceApi.uploadDocument(correspondenceId, file),
    {
      successMessage: "تم رفع المرفق بنجاح",
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ["correspondence", "documents", correspondenceId] })
    }
  );

  const handleFileUpload = (file: File | null) => {
    if (!file) return;
    uploadMutation.mutate(file);
  };

  const deleteDocMutation = useApiMutation(
    (docId: number) => correspondenceApi.deleteDocument(correspondenceId, docId),
    {
      successMessage: "تم حذف المرفق",
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ["correspondence", "documents", correspondenceId] })
    }
  );

  const handleDocDelete = useCallback(async (docId: number, fileName: string) => {
    const ok = await confirm({
      title: "حذف المرفق",
      description: `هل أنت متأكد من حذف الملف "${fileName}"؟`,
      variant: "danger"
    });
    if (ok) deleteDocMutation.mutate(docId);
  }, [confirm, deleteDocMutation]);

  const docColumns: CustomColumnDef<HrDocumentDto>[] = [
    {
      key: "fileName",
      header: "اسم الملف",
      render: (row) => (
        <div className="flex items-center gap-2">
          <FileText className="h-4 w-4 text-primary" />
          <span className="text-sm font-medium">{row.fileName}</span>
        </div>
      )
    },
    {
      key: "uploadedByUser",
      header: "بواسطة",
      render: (row) => <span className="text-xs text-muted-foreground">{row.uploadedByUser}</span>
    },
    {
      key: "createdAt",
      header: "تاريخ الرفع",
      render: (row) => <span className="font-mono text-[10px] sm:text-xs">{formatDate(row.createdAt)}</span>
    },
    {
      key: "actions",
      header: "الإجراءات",
      className: "w-20",
      render: (row) => (
        <div className="flex items-center justify-end gap-1">
          <Button
            size="icon"
            variant="ghost"
            className="min-h-[44px] min-w-[44px] text-primary"
            title="تحميل"
            onClick={() => window.open(getDocumentUrl(row.filePath) ?? "", "_blank")}
          >
            <Download className="h-4 w-4" />
          </Button>
          <Button
            size="icon"
            variant="ghost"
            className="min-h-[44px] min-w-[44px] text-destructive"
            title="حذف"
            onClick={() => handleDocDelete(row.documentID, row.fileName)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      )
    }
  ];

  if (isLoading) return <CorrespondenceDetailsSkeleton />;
  if (!correspondence) return <div className="p-20 text-center">القيد غير موجود</div>;

  const directionLabel = correspondence.direction === CorrespondenceDirection.Incoming ? "وارد" : "صادر";
  const directionVariant: "info" | "success" = correspondence.direction === CorrespondenceDirection.Incoming ? "info" : "success";
  const backPath = correspondence.direction === CorrespondenceDirection.Incoming ? "/correspondence/incoming" : "/correspondence/outgoing";

  return (
    <DetailLayout
      breadcrumb={
        <Breadcrumb
          items={[
            { label: "الموارد البشرية", href: "/hr" },
            { label: `سجل ال${directionLabel}`, href: backPath },
            { label: correspondence.referenceNumber }
          ]}
        />
      }
      headerTitle={
        <div className="flex items-center gap-4 text-start">
          <Button variant="outline" size="icon" onClick={() => navigate(backPath)} className="rounded-full h-10 w-10 ">
            <ArrowRight className="h-5 w-5" />
          </Button>
          <div>
            <div className="flex items-center gap-2 mb-1">
              <Badge variant={directionVariant} className="font-black px-2 py-0 h-5 text-[10px] sm:text-xs">{directionLabel}</Badge>
              <span className="text-[10px] sm:text-xs text-muted-foreground font-bold tracking-wider">#{correspondence.sequentialNumber}</span>
            </div>
            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-black text-foreground tracking-tight">تفاصيل القيد - {correspondence.referenceNumber}</Heading>
          </div>
        </div>
      }
      headerActions={
        <div className="flex gap-2">
          {canManage && (
            <Button variant="outline" onClick={() => crud.openEdit(correspondence)} className="gap-2  font-bold">
              <Edit className="h-4 w-4" /> تعديل
            </Button>
          )}
          {canManage && (
            <Button variant="destructive" onClick={handleDelete} loading={deleteMutation.isPending}>
              <Trash2 className="h-4 w-4" /> حذف
            </Button>
          )}
        </div>
      }
      sidebar={
        <Card className="p-5 border-border bg-card space-y-6">
          <div className="flex items-center gap-2 text-primary border-b border-border pb-3">
            <Info className="h-4 w-4" />
            <Heading level="h3" className="font-black text-[10px] sm:text-xs uppercase tracking-widest text-start">معلومات المراجعة</Heading>
          </div>
          <div className="space-y-4 text-start" dir="rtl">
            <InfoPair label="أُنشئ بواسطة" value={correspondence.createdByUserName} icon={<User className="w-3.5 h-3.5" />} />
            <InfoPair label="تاريخ الإنشاء" value={formatDate(correspondence.createdAt)} icon={<Clock className="w-3.5 h-3.5" />} />
            {correspondence.documentCount > 0 && (
              <InfoPair label="عدد المرفقات" value={`${correspondence.documentCount} ملفات`} icon={<FileText className="w-3.5 h-3.5" />} />
            )}
          </div>
        </Card>
      }
    >
      <div className="space-y-6" dir="rtl">
        <Card className="overflow-hidden border-2">
          <div className="p-4 bg-muted/30 border-b border-border flex items-center gap-2">
            <Hash className="h-4 w-4 text-primary" />
            <Heading level="h2" className="font-black text-sm">البيانات الأساسية للقيد</Heading>
          </div>
          <div className="p-6 text-start">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-y-8 gap-x-12">
              <InfoPair label="نوع المراسلة" value={correspondence.typeName} icon={<Info className="w-3.5 h-3.5" />} />
              <InfoPair label="تاريخ المراسلة" value={formatDate(correspondence.correspondenceDate)} icon={<CalendarDays className="w-3.5 h-3.5" />} mono />
              <InfoPair label="رقم القيد" value={correspondence.referenceNumber} icon={<Hash className="w-3.5 h-3.5" />} mono />

              <InfoPair label="الموضوع" value={correspondence.subject} icon={<FileText className="w-3.5 h-3.5" />} className="md:col-span-2 lg:col-span-3" />

              {correspondence.externalPartyName && (
                <InfoPair label="الجهة الخارجية" value={correspondence.externalPartyName} icon={<Building2 className="w-3.5 h-3.5" />} />
              )}

              {correspondence.employeeName && (
                <InfoPair label="الموظف المرتبط" value={correspondence.employeeName} icon={<User className="w-3.5 h-3.5" />} />
              )}

              {correspondence.pageNumber && (
                <InfoPair label="رقم الصفحة (ص)" value={correspondence.pageNumber} icon={<Hash className="w-3.5 h-3.5" />} />
              )}
            </div>

            {correspondence.notes && (
<<<<<<< HEAD
              <div className="mt-8 pt-6 border-t border-dashed border-border">
                <p className="text-[10px] sm:text-xs text-muted-foreground uppercase font-black mb-2">ملاحظات إضافية</p>
                <Text variant="body" className="leading-relaxed text-foreground bg-muted/20 p-4 rounded-xl border italic">
=======
              <div className="mt-8 pt-6 border-t border-dashed border-border/60">
                <p className="text-[10px] sm:text-xs text-muted-foreground uppercase font-black mb-2">ملاحظات إضافية</p>
                <Text variant="body" className="leading-relaxed text-foreground/80 bg-muted/20 p-4 rounded-xl border italic">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                  {correspondence.notes}
                </Text>
              </div>
            )}
          </div>
        </Card>

        {/* Attachments Section */}
        <Card>
          <div className="p-4 bg-muted/30 border-b border-border flex justify-between items-center">
            <div className="flex items-center gap-2">
              <FileText className="h-4 w-4 text-primary" />
              <Heading level="h2" className="font-black text-sm">المرفقات والوثائق</Heading>
            </div>
            <div className="flex items-center gap-3">
              {canUpload && (
                <FileUploader
                  onChange={handleFileUpload}
                  uploading={uploadMutation.isPending}
                />
              )}
            </div>
          </div>
          <DataTable
            columns={docColumns}
            data={documents}
            loading={loadingDocs}
            emptyMessage="لا توجد مرفقات لهذا القيد"
          />
        </Card>
      </div>

      <CorrespondenceFormDialog
        open={crud.isFormOpen}
        onOpenChange={crud.onOpenChange}
        direction={correspondence.direction as CorrespondenceDirection}
        correspondence={correspondence}
        onSaved={() => queryClient.invalidateQueries({ queryKey: ["correspondence", "detail", correspondenceId] })}
      />

      {ConfirmDialogElement}
    </DetailLayout>
  );
}

function CorrespondenceDetailsSkeleton() {
  return (
    <div className="space-y-6 p-6">
      <Skeleton className="h-8 w-48 rounded-lg mb-8" />
      <Skeleton className="h-40 w-full rounded-2xl" />
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 mt-8">
        <Skeleton className="lg:col-span-1 h-64 rounded-2xl" />
        <Skeleton className="lg:col-span-3 h-96 rounded-2xl" />
      </div>
    </div>
  );
}
