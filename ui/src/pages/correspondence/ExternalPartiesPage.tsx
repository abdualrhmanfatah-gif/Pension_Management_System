import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, Edit, Trash2 } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Controller,useForm } from "react-hook-form";

import { correspondenceApi } from "@/api/correspondence";
import { 
  Badge, 
  BaseFormDialog, 
  Button,
  ConfirmDialog,
  CrudPageLayout, 
  type CustomColumnDef,
  FormField,
  Input, 
  NativeSelect, 
  Textarea} from "@/components/ui";
import { useApiMutation, useCrudState, useDialogReset } from "@/hooks";
import { 
  type CreateExternalPartyDto, 
  type ExternalPartyDto, 
  externalPartySchema 
} from "@/types/correspondence";

/**
 * صفحة إدارة الجهات الخارجية
 */
export function ExternalPartiesPage() {
  const queryClient = useQueryClient();
  const crud = useCrudState<ExternalPartyDto>();
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);
  const searchInputRef = useRef<HTMLInputElement>(null);

  // fetching data
  const { data: parties, isLoading } = useQuery({
    queryKey: ["correspondence", "external-parties"],
    queryFn: () => correspondenceApi.getExternalParties().then(res => res.data || [])
  });

  // mutations
  const createMutation = useApiMutation(
    (data: CreateExternalPartyDto) => correspondenceApi.createExternalParty(data),
    {
      successMessage: "تم إضافة الجهة بنجاح",
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["correspondence", "external-parties"] });
        crud.closeForm();
      }
    }
  );

  const updateMutation = useApiMutation(
    (data: CreateExternalPartyDto) => correspondenceApi.updateExternalParty(crud.selectedItem!.externalPartyID, data),
    {
      successMessage: "تم تحديث بيانات الجهة بنجاح",
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["correspondence", "external-parties"] });
        crud.closeForm();
      }
    }
  );

  const deleteMutation = useApiMutation(
    (id: number) => correspondenceApi.deleteExternalParty(id),
    {
      successMessage: "تم حذف الجهة بنجاح",
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["correspondence", "external-parties"] });
      }
    }
  );

  // table columns
  const columns: CustomColumnDef<ExternalPartyDto>[] = useMemo(() => [
    {
      key: "name",
      header: "اسم الجهة",
      render: (row) => <span className="font-bold">{row.name}</span>
    },
    {
      key: "partyType",
      header: "نوع الجهة",
      render: (row) => (
        <Badge variant="outline" className="font-medium bg-blue-50 text-blue-700 border-blue-200">
          {row.partyType}
        </Badge>
      )
    },
    {
      key: "contactInfo",
      header: "معلومات التواصل",
      accessor: (row) => row.contactInfo || "—"
    },
    {
      key: "isActive",
      header: "الحالة",
      render: (row) => (
        <Badge variant={row.isActive ? "success" : "ghost"}>
          {row.isActive ? "فعال" : "غير فعال"}
        </Badge>
      )
    },
    {
      key: "actions",
      header: "الإجراءات",
      className: "w-24",
      render: (row) => (
        <div className="flex items-center gap-1">
          <Button 
            variant="ghost" 
            size="icon" 
            className="min-h-[44px] min-w-[44px] text-primary" 
            onClick={() => crud.openEdit(row)}
            title="تعديل"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button 
            variant="ghost" 
            size="icon" 
            className="min-h-[44px] min-w-[44px] text-destructive" 
            onClick={() => crud.openDelete(row)}
            title="حذف"
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      )
    }
  ], [crud]);

  // ── Keyboard Navigation ────────────────────────────
  const handleSearchKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
    const items = parties || [];
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setFocusedIndex(prev => {
        const next = prev + 1;
        const clamped = next < items.length ? next : prev;
        focusedIndexRef.current = clamped;
        return clamped;
      });
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setFocusedIndex(prev => {
        const clamped = prev > 0 ? prev - 1 : prev;
        focusedIndexRef.current = clamped;
        return clamped;
      });
    } else if (e.key === "Enter") {
      e.preventDefault();
      const idx = focusedIndexRef.current;
      if (idx >= 0 && idx < items.length) {
        crud.openEdit(items[idx]);
      }
    } else if (e.key === "Escape") {
      setFocusedIndex(-1);
      focusedIndexRef.current = -1;
    }
  }, [parties, crud]);

  // Scroll to focused row
  useEffect(() => {
    if (focusedIndex >= 0) {
      const row = document.querySelector(`[data-row-index="${focusedIndex}"]`)
                || document.querySelector(`[data-index="${focusedIndex}"]`);
      if (row) {
        row.scrollIntoView({ behavior: "smooth", block: "nearest" });
      }
    }
  }, [focusedIndex]);

  const handleDelete = () => {
    if (crud.deleteTarget) {
      deleteMutation.mutate(crud.deleteTarget.externalPartyID);
    }
  };

  return (
    <CrudPageLayout
      title="إدارة الجهات الخارجية"
      description="إدارة الجهات والمنظمات التي يتم تبادل المراسلات معها"
      icon={<Building2 className="h-6 w-6 text-primary" />}
      createLabel="جهة جديدة"
      onCreateClick={crud.openCreate}
      columns={columns}
      data={parties || []}
      loading={isLoading}
      searchable
      searchInHeader
      searchInputRef={searchInputRef}
      onSearchKeyDown={handleSearchKeyDown}
      focusedRowIndex={focusedIndex}
    >
      <ExternalPartyFormDialog 
        open={crud.isFormOpen}
        onOpenChange={crud.onOpenChange}
        selectedItem={crud.selectedItem}
        onSubmit={crud.isEditing ? updateMutation.mutate : createMutation.mutate}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <ConfirmDialog
        open={crud.isDeleteOpen}
        onOpenChange={(o) => !o && crud.closeDelete()}
        title="حذف الجهة"
        description={`هل أنت متأكد من حذف الجهة "${crud.deleteTarget?.name}"؟ لا يمكن التراجع عن هذا الإجراء.`}
        onConfirm={handleDelete}
        isLoading={deleteMutation.isPending}
        variant="danger"
      />
    </CrudPageLayout>
  );
}

// ─── Internal Components ───────────────────────────────

interface FormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  selectedItem: ExternalPartyDto | null;
  onSubmit: (data: CreateExternalPartyDto) => void;
  isLoading: boolean;
}

function ExternalPartyFormDialog({ open, onOpenChange, selectedItem, onSubmit, isLoading }: FormDialogProps) {
  const isEdit = !!selectedItem;
  
  const { 
    register, 
    handleSubmit, 
    control, 
    reset, 
    formState: { errors, isDirty } 
  } = useForm<CreateExternalPartyDto>({
    resolver: zodResolver(externalPartySchema),
    defaultValues: {
      name: "",
      partyType: "وزارة",
      contactInfo: ""
    }
  });

  useDialogReset(open, reset, selectedItem ? {
    name: selectedItem.name,
    partyType: selectedItem.partyType,
    contactInfo: selectedItem.contactInfo || ""
  } : undefined);

  const typeOptions = [
    { label: "بنك", value: "بنك" },
    { label: "وزارة", value: "وزارة" },
    { label: "إدارة أمنية", value: "إدارة أمنية" },
    { label: "هيئة حكومية", value: "هيئة حكومية" },
    { label: "أخرى", value: "أخرى" }
  ];

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEdit ? "تعديل بيانات الجهة" : "إضافة جهة خارجية جديدة"}
      isLoading={isLoading}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      formErrors={errors}
    >
      <div className="space-y-4" dir="rtl">
        <Input 
          label="اسم الجهة"
          placeholder="مثلاً: وزارة الداخلية"
          {...register("name")}
          error={errors.name?.message}
          required
        />

        <FormField label="نوع الجهة" error={errors.partyType?.message} required>
          <Controller 
            name="partyType"
            control={control}
            render={({ field }) => (
              <NativeSelect 
                options={typeOptions}
                value={field.value}
                onChange={field.onChange}
                placeholder="اختر النوع..."
              />
            )}
          />
        </FormField>

        <Textarea 
          label="معلومات التواصل"
          placeholder="أرقام هواتف، عناوين، بريد إلكتروني..."
          {...register("contactInfo")}
          error={errors.contactInfo?.message}
          rows={3}
        />
      </div>
    </BaseFormDialog>
  );
}
