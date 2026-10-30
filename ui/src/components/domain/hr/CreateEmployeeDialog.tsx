// use no memo removed for Vite compatibility
import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Briefcase, Camera,FileText, MapPin, Shield, User } from "lucide-react";
import { useEffect, useMemo } from "react";
import { Controller, type Resolver,type SubmitHandler, useForm, useWatch } from "react-hook-form";
import * as z from "zod";

<<<<<<< HEAD
=======
import { getPhotoUrl } from "@/lib/utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { type CreateEmployeeDto, type EmployeeDetailsDto,hrApi } from "@/api/hr";
import { 
  BaseFormDialog, 
  DateInput,
  FileField,
  FormField,
  FormSection,
  Input, 
  nationalIdSchema,
  NativeSelect,
  TabbedFormSection,
  Textarea,
  yemenPhoneSchema,
} from "@/components/ui";
<<<<<<< HEAD
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
import { getPhotoUrl } from "@/lib/utils";
=======
import { useApiMutation } from "@/hooks";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ─── Validation Schema ─────────────────────────────────

const employeeSchema = z.object({
  employeeNumber: z.string().trim().min(1, "الرقم الوظيفي مطلوب"),
  fullName: z.string().trim().min(3, "الاسم الكامل يجب أن يكون 3 أحرف على الأقل"),
  nationalID: nationalIdSchema,
  departmentID: z.string().min(1, "الإدارة مطلوبة"),
  sectionID: z.string().optional().or(z.literal("")),
  jobTitleID: z.string().min(1, "المسمى الوظيفي مطلوب"),
  employmentStatusID: z.string().min(1, "حالة الخدمة مطلوبة"),
  gender: z.string().min(1, "الجنس مطلوب"),
  hireDate: z.string().min(1, "تاريخ التعيين مطلوب"),
  birthDate: z.string().optional().or(z.literal("")),
  phone: yemenPhoneSchema.optional().or(z.literal("")),
  address: z.string().trim().optional().or(z.literal("")),
  birthPlace: z.string().trim().optional().or(z.literal("")),
  maritalStatus: z.string().optional().or(z.literal("")),
  militaryNumber: z.string().trim().optional().or(z.literal("")),
  rankID: z.string().optional().or(z.literal("")),
  militaryServiceDate: z.string().optional().or(z.literal("")),
  jobPositions: z.string().trim().optional().or(z.literal("")),
  academicQualifications: z.string().trim().optional().or(z.literal("")),
  notes: z.string().trim().optional().or(z.literal("")),
  bloodGroup: z.string().optional().or(z.literal("")),
  nationalIDIssuePlace: z.string().trim().optional().or(z.literal("")),
  nationalIDIssueDate: z.string().optional().or(z.literal("")),
  nationalIDExpiryDate: z.string().optional().or(z.literal("")),
  emergencyContactName: z.string().trim().optional().or(z.literal("")),
  emergencyContactPhone: z.string().trim().optional().or(z.literal("")),
  profilePhoto: z.any().optional(),
});

type EmployeeFormValues = z.infer<typeof employeeSchema>;

const DRAFT_KEY = "employee_draft";

const TAB_FIELDS: Record<string, (keyof EmployeeFormValues)[]> = {
  personal: ["fullName", "employeeNumber", "nationalID", "gender", "bloodGroup",
    "nationalIDIssuePlace", "nationalIDIssueDate", "nationalIDExpiryDate",
    "birthPlace", "birthDate", "maritalStatus"],
  employment: ["departmentID", "sectionID", "jobTitleID", "employmentStatusID", "hireDate", "jobPositions"],
  additional: ["phone", "address", "militaryNumber", "rankID", "militaryServiceDate",
    "academicQualifications", "notes", "emergencyContactName", "emergencyContactPhone"],
};

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSaved?: () => void;
  employeeId?: number | null;
}

/**
 * Enterprise-grade CreateEmployeeDialog.
 * Fixed: Unified Props API (open/onOpenChange) and cleaned Zod schema.
 */
export function CreateEmployeeDialog({ open, onOpenChange, onSaved, employeeId }: Props) {
  const isEdit = !!employeeId;

  const form = useForm<EmployeeFormValues>({
    resolver: zodResolver(employeeSchema) as Resolver<z.infer<typeof employeeSchema>>,
    defaultValues: {
      employeeNumber: "",
      fullName: "",
      nationalID: "",
      departmentID: "",
      sectionID: "",
      jobTitleID: "",
      employmentStatusID: "",
      gender: "1",
      hireDate: new Date().toISOString().split("T")[0],
      phone: "",
      address: "",
      birthPlace: "",
      maritalStatus: "",
      militaryNumber: "",
      rankID: "",
      militaryServiceDate: "",
      jobPositions: "",
      academicQualifications: "",
      notes: "",
      profilePhoto: null,
      bloodGroup: "",
      nationalIDIssuePlace: "",
      nationalIDIssueDate: "",
      nationalIDExpiryDate: "",
      emergencyContactName: "",
      emergencyContactPhone: "",
    },
  });

  const { control, handleSubmit, reset, trigger, formState: { errors, isDirty } } = form;
  const selectedDepartmentId = useWatch({ control, name: "departmentID" });
  const allValues = useWatch({ control });

  // Compute error count per tab
  const tabErrors = useMemo(() => {
    const result: Record<string, number> = {};
    for (const [tabId, fields] of Object.entries(TAB_FIELDS)) {
      result[tabId] = fields.filter(f => errors[f]).length;
    }
    return result;
  }, [errors]);

  // Validate current tab's fields on tab switch
  const handleTabChange = useMemo(() => (tabId: string) => {
    const fields = TAB_FIELDS[tabId];
    if (fields) trigger(fields);
  }, [trigger]);

  // Restore draft on open in create mode
  useEffect(() => {
    if (open && !isEdit) {
      const saved = sessionStorage.getItem(DRAFT_KEY);
      if (saved) {
        try {
          const draft = JSON.parse(saved);
          reset(draft);
        } catch {
          // ignore corrupt draft
        }
      }
    }
  }, [open, isEdit, reset]);

  // Save draft to sessionStorage on changes (create mode only)
  useEffect(() => {
    if (isEdit) return;
    const { profilePhoto: _p, ...textFields } = allValues;
    void _p;
    sessionStorage.setItem(DRAFT_KEY, JSON.stringify(textFields));
  }, [allValues, isEdit]);

  // Fetch Employee Data for Edit Mode
  const { data: employeeData, isLoading: isLoadingEmployee } = useQuery({
    queryKey: queryKeys.hr.employees.detail(employeeId!),
    queryFn: () => hrApi.getEmployeeById(employeeId!),
    enabled: open && isEdit,
  });

  // Reset form when employee data is loaded
  useEffect(() => {
    if (isEdit && employeeData) {
      reset({
        employeeNumber: employeeData.employeeNumber || "",
        fullName: employeeData.fullName || "",
        nationalID: employeeData.nationalID || "",
        departmentID: String(employeeData.departmentID || ""),
        sectionID: employeeData.sectionID ? String(employeeData.sectionID) : "",
        jobTitleID: String(employeeData.jobTitleID || ""),
        employmentStatusID: String(employeeData.employmentStatusID || ""),
        gender: String(employeeData.gender || "1"),
        hireDate: employeeData.hireDate ? employeeData.hireDate.split("T")[0] : "",
        birthDate: employeeData.birthDate ? employeeData.birthDate.split("T")[0] : "",
        phone: employeeData.phone || "",
        address: employeeData.address || "",
        birthPlace: employeeData.birthPlace || "",
        maritalStatus: employeeData.maritalStatus || "",
        militaryNumber: employeeData.militaryNumber || "",
        rankID: employeeData.rankID ? String(employeeData.rankID) : "",
        militaryServiceDate: employeeData.militaryServiceDate ? employeeData.militaryServiceDate.split("T")[0] : "",
        jobPositions: employeeData.jobPositions || "",
        academicQualifications: employeeData.academicQualifications || "",
        notes: employeeData.notes || "",
        profilePhoto: employeeData.profilePhotoPath ? getPhotoUrl(employeeData.profilePhotoPath) : null,
        bloodGroup: employeeData.bloodGroup || "",
        nationalIDIssuePlace: employeeData.nationalIDIssuePlace || "",
        nationalIDIssueDate: employeeData.nationalIDIssueDate ? employeeData.nationalIDIssueDate.split("T")[0] : "",
        nationalIDExpiryDate: employeeData.nationalIDExpiryDate ? employeeData.nationalIDExpiryDate.split("T")[0] : "",
        emergencyContactName: employeeData.emergencyContactName || "",
        emergencyContactPhone: employeeData.emergencyContactPhone || "",
      });
    }
  }, [isEdit, employeeData, reset]);

  // Reference Data Queries
  const { data: departments } = useQuery({
    queryKey: queryKeys.hr.departments,
    queryFn: () => hrApi.getDepartments(),
    enabled: open,
  });

  const { data: sections } = useQuery({
    queryKey: queryKeys.hr.sections.byDepartment(Number(selectedDepartmentId)),
    queryFn: () => hrApi.getSections(Number(selectedDepartmentId)),
    enabled: open && !!selectedDepartmentId,
  });

  const { data: jobTitles } = useQuery({
    queryKey: queryKeys.hr.jobTitles,
    queryFn: () => hrApi.getJobTitles(),
    enabled: open,
  });

  const { data: employmentStatuses } = useQuery({
    queryKey: queryKeys.hr.employmentStatuses,
    queryFn: () => hrApi.getEmploymentStatuses(),
    enabled: open,
  });

  const { data: ranks } = useQuery({
    queryKey: queryKeys.hr.ranks,
    queryFn: () => hrApi.getRanks(),
    enabled: open,
  });

  const mutation = useApiMutation<EmployeeDetailsDto, EmployeeFormValues>(
    async (values: EmployeeFormValues) => {
      const payload: CreateEmployeeDto = {
        employeeNumber: values.employeeNumber,
        fullName: values.fullName,
        nationalID: values.nationalID,
        departmentID: Number(values.departmentID),
        sectionID: values.sectionID ? Number(values.sectionID) : null,
        jobTitleID: Number(values.jobTitleID),
        employmentStatusID: Number(values.employmentStatusID),
        phone: values.phone || undefined,
        birthPlace: values.birthPlace,
        maritalStatus: values.maritalStatus,
        militaryNumber: values.militaryNumber,
        rankID: values.rankID ? Number(values.rankID) : null,
        militaryServiceDate: values.militaryServiceDate || null,
        jobPositions: values.jobPositions,
        academicQualifications: values.academicQualifications,
        address: values.address || undefined,
        notes: values.notes || undefined,
        bloodGroup: values.bloodGroup || undefined,
        nationalIDIssuePlace: values.nationalIDIssuePlace || undefined,
        nationalIDIssueDate: values.nationalIDIssueDate || null,
        nationalIDExpiryDate: values.nationalIDExpiryDate || null,
        emergencyContactName: values.emergencyContactName || undefined,
        emergencyContactPhone: values.emergencyContactPhone || undefined,
        gender: Number(values.gender),
        hireDate: values.hireDate,
        birthDate: values.birthDate || null,
      };

      let result: EmployeeDetailsDto;
      if (isEdit && employeeId) {
        result = await hrApi.updateEmployee(employeeId, payload);
      } else {
        result = await hrApi.createEmployee(payload);
      }

      // Handle photo upload if a new file is selected
      if (values.profilePhoto instanceof File) {
        await hrApi.uploadEmployeePhoto(result.employeeID, values.profilePhoto);
      }

      return result;
    },
    {
      successMessage: isEdit ? "تم تحديث بيانات الموظف بنجاح" : "تم تسجيل الموظف الجديد بنجاح",
      invalidateKeys: [["hr", "employees"], ["hr", "employee", employeeId]],
      onSuccess: () => {
        sessionStorage.removeItem(DRAFT_KEY);
        if (onSaved) onSaved();
        onOpenChange(false);
      }
    }
  );

  const onSubmit: SubmitHandler<EmployeeFormValues> = (data) => {
    mutation.mutate(data);
  };

  const tabs = [
    {
      id: "personal",
      label: "البيانات الشخصية",
      icon: <User className="h-4 w-4" />,
      content: (
        <div className="space-y-6">
          <div className="flex flex-col lg:flex-row gap-6">
            <div className="flex-1 space-y-6">
              <FormSection title="المعلومات التعريفية" icon={User}>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="md:col-span-2">
                    <Input
                      label="الاسم الكامل"
                      placeholder="الاسم الرباعي واللقب"
                      {...form.register("fullName")}
                      error={errors.fullName?.message}
                      required
                    />
                  </div>
                  <Input
                    label="الرقم الوظيفي"
                    placeholder="مثال: 12345"
                    {...form.register("employeeNumber")}
                    error={errors.employeeNumber?.message}
                    required
                  />
                  <Input
                    label="الرقم الوطني"
                    placeholder="1010xxxxxx"
                    {...form.register("nationalID")}
                    error={errors.nationalID?.message}
                    required
                  />
                  <FormField label="الجنس" error={errors.gender?.message} required>
                    <Controller
                      name="gender"
                      control={control}
                      render={({ field }) => (
                        <NativeSelect
                          options={[{ label: "ذكر", value: "1" }, { label: "أنثى", value: "2" }]}
                          value={field.value}
                          onChange={field.onChange}
                        />
                      )}
                    />
                  </FormField>
                  <FormField label="فصيلة الدم" error={errors.bloodGroup?.message}>
                    <Controller
                      name="bloodGroup"
                      control={control}
                      render={({ field }) => (
                        <NativeSelect
                          options={[
                            { label: "A+", value: "A+" },
                            { label: "A-", value: "A-" },
                            { label: "B+", value: "B+" },
                            { label: "B-", value: "B-" },
                            { label: "AB+", value: "AB+" },
                            { label: "AB-", value: "AB-" },
                            { label: "O+", value: "O+" },
                            { label: "O-", value: "O-" }
                          ]}
                          value={field.value || ""}
                          onChange={field.onChange}
                          placeholder="اختر الفصيلة..."
                        />
                      )}
                    />
                  </FormField>
                </div>
              </FormSection>
            </div>

            <div className="w-full lg:w-56 shrink-0">
              <FormSection title="الصورة" icon={Camera}>
                <div className="flex justify-center">
                  <FileField
                    name="profilePhoto"
                    control={control}
                    label="تغيير"
                    accept="image/*"
                    className="w-full h-56 md:h-64 rounded-3xl"
                  />
                </div>
              </FormSection>
            </div>
          </div>

          <FormSection title="تفاصيل الهوية ومحل الإقامة" icon={Shield}>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <Input
                label="مكان إصدار الهوية"
                placeholder="..."
                {...form.register("nationalIDIssuePlace")}
                error={errors.nationalIDIssuePlace?.message}
              />
              <Controller
                name="nationalIDIssueDate"
                control={control}
                render={({ field }) => (
                  <DateInput
                    label="تاريخ الإصدار"
                    value={field.value || undefined}
                    onChange={field.onChange}
                    error={errors.nationalIDIssueDate?.message}
                  />
                )}
              />
              <Controller
                name="nationalIDExpiryDate"
                control={control}
                render={({ field }) => (
                  <DateInput
                    label="تاريخ الانتهاء"
                    value={field.value || undefined}
                    onChange={field.onChange}
                    error={errors.nationalIDExpiryDate?.message}
                  />
                )}
              />
              <Input
                label="مكان الميلاد"
                placeholder="المدينة / المحافظة"
                {...form.register("birthPlace")}
                error={errors.birthPlace?.message}
              />
              <Controller
                name="birthDate"
                control={control}
                render={({ field }) => (
                  <DateInput
                    label="تاريخ الميلاد"
                    value={field.value || undefined}
                    onChange={field.onChange}
                    error={errors.birthDate?.message}
                  />
                )}
              />
              <FormField label="الحالة الاجتماعية" error={errors.maritalStatus?.message}>
                <Controller
                  name="maritalStatus"
                  control={control}
                  render={({ field }) => (
                    <NativeSelect
                      options={[
                        { label: "أعزب", value: "أعزب" },
                        { label: "متزوج", value: "متزوج" },
                        { label: "مطلق", value: "مطلق" },
                        { label: "أرمل", value: "أرمل" }
                      ]}
                      value={field.value || ""}
                      onChange={field.onChange}
                      placeholder="اختر الحالة..."
                    />
                  )}
                />
              </FormField>
            </div>
          </FormSection>
        </div>
      )
    },
    {
      id: "employment",
      label: "التبعية والعمل",
      icon: <Briefcase className="h-4 w-4" />,
      content: (
        <FormSection title="التسكين الإداري" icon={Briefcase}>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <FormField label="الإدارة" error={errors.departmentID?.message} required>
              <Controller
                name="departmentID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={departments?.map(d => ({ label: d.name, value: String(d.departmentID) })) || []}
                    value={field.value}
                    onChange={(val) => {
                      field.onChange(val);
                      form.setValue("sectionID", "");
                    }}
                    placeholder="اختر الإدارة..."
                  />
                )}
              />
            </FormField>
            <FormField label="القسم" error={errors.sectionID?.message}>
              <Controller
                name="sectionID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={sections?.map(s => ({ label: s.name, value: String(s.sectionID) })) || []}
                    value={field.value || ""}
                    onChange={field.onChange}
                    disabled={!selectedDepartmentId}
                    placeholder="اختر القسم..."
                  />
                )}
              />
            </FormField>
            <FormField label="المسمى الوظيفي" error={errors.jobTitleID?.message} required>
              <Controller
                name="jobTitleID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={jobTitles?.map(j => ({ label: j.name, value: String(j.jobTitleID) })) || []}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </FormField>
            <FormField label="حالة الخدمة" error={errors.employmentStatusID?.message} required>
              <Controller
                name="employmentStatusID"
                control={control}
                render={({ field }) => (
                  <NativeSelect
                    options={employmentStatuses?.map(es => ({ label: es.name, value: String(es.employmentStatusID) })) || []}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </FormField>
            <Controller
              name="hireDate"
              control={control}
              render={({ field }) => (
                <DateInput
                  label="تاريخ التعيين"
                  value={field.value || undefined}
                  onChange={field.onChange}
                  error={errors.hireDate?.message}
                  required
                />
              )}
            />
            <Input
              label="المناصب الوظيفية السابقة/الحالية"
              placeholder="مثال: مدير إدارة، رئيس قسم..."
              {...form.register("jobPositions")}
              error={errors.jobPositions?.message}
            />
          </div>
        </FormSection>
      )
    },
    {
      id: "additional",
      label: "بيانات أخرى",
      icon: <FileText className="h-4 w-4" />,
      content: (
        <div className="space-y-4">
          <FormSection title="العنوان والتواصل" icon={MapPin}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label="رقم الهاتف"
                placeholder="7xxxxxxx"
                {...form.register("phone")}
                error={errors.phone?.message}
              />
              <Input
                label="العنوان"
                placeholder="المدينة، الشارع"
                {...form.register("address")}
                error={errors.address?.message}
              />
            </div>
          </FormSection>
          <FormSection title="البيانات العسكرية" icon={Shield}>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Input
                label="الرقم العسكري"
                placeholder="أدخل الرقم العسكري إن وجد..."
                {...form.register("militaryNumber")}
                error={errors.militaryNumber?.message}
              />
              <FormField label="الرتبة" error={errors.rankID?.message}>
                <Controller
                  name="rankID"
                  control={control}
                  render={({ field }) => (
                    <NativeSelect
                      options={ranks?.map(r => ({ label: r.rankName, value: String(r.rankID) })) || []}
                      value={field.value || ""}
                      onChange={field.onChange}
                      placeholder="اختر الرتبة..."
                    />
                  )}
                />
              </FormField>
              <Controller
                name="militaryServiceDate"
                control={control}
                render={({ field }) => (
                  <DateInput
                    label="تاريخ الخدمة العسكرية"
                    value={field.value || undefined}
                    onChange={field.onChange}
                    error={errors.militaryServiceDate?.message}
                  />
                )}
              />
            </div>
          </FormSection>
          <FormSection title="ملاحظات تكميلية" icon={FileText}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="md:col-span-2">
                <Textarea
                  label="المؤهلات العلمية"
                  placeholder="أدخل المؤهلات الأكاديمية بالتفصيل (الجامعة، التخصص، السنة)..."
                  {...form.register("academicQualifications")}
                  error={errors.academicQualifications?.message}
                  rows={3}
                />
              </div>
              <div className="md:col-span-2">
                <Textarea
                  label="ملاحظات إدارية"
                  placeholder="..."
                  {...form.register("notes")}
                  error={errors.notes?.message}
                  rows={2}
                />
              </div>
            </div>
          </FormSection>

          <FormSection title="بيانات التواصل عند الطوارئ" icon={User}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label="اسم شخص للتواصل"
                placeholder="اسم القريب أو الصديق..."
                {...form.register("emergencyContactName")}
                error={errors.emergencyContactName?.message}
              />
              <Input
                label="رقم هاتف الطوارئ"
                placeholder="7xxxxxxx"
                {...form.register("emergencyContactPhone")}
                error={errors.emergencyContactPhone?.message}
              />
            </div>
          </FormSection>
        </div>
      )
    }
  ];

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title={isEdit ? `تعديل بيانات: ${employeeData?.fullName || ""}` : "إضافة موظف جديد"}
      description={isEdit ? "تحديث السجل الوظيفي والبيانات الأساسية" : "تعبئة بيانات الكادر الوظيفي الجديد في المنظومة"}
      size="xl"
      isLoading={mutation.isPending || (isEdit && isLoadingEmployee)}
      isDirty={isDirty}
      onSubmit={handleSubmit(onSubmit)}
      resetOnOpen={!isEdit}
      reset={reset}
      showGlobalError
      formErrors={errors}
      submitLabel={isEdit ? "حفظ التعديلات" : "حفظ بيانات الموظف"}
    >
      <TabbedFormSection tabs={tabs} isDialogOpen={open} tabErrors={tabErrors} onTabChange={handleTabChange} />
    </BaseFormDialog>
  );
}
