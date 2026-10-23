import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Building2,
  File as FileIcon,
  Paperclip,
  SendHorizonal,
  Upload,
  User,
  Users,
  X,
} from "lucide-react";
import { useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";

import api from "@/api/axios";
import {
  type AdminBroadcastNotificationDto,
  notificationsApi,
} from "@/api/notifications";
import {
  BaseFormDialog,
  type BaseOverlayProps,
  Button,
  FormField,
  FormSection,
  Input,
  NativeSelect,
  Text,
  Textarea,
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import type { ApiResponse } from "@/types/api";
import { CATEGORY_LABELS } from "@/types/notifications";

interface RoleOption {
  roleID: number;
  roleName: string;
}
interface BranchOption {
  id: number;
  name: string;
}

const sendNotificationSchema = z.object({
  sendType: z.enum(["user", "role", "branch", "multi-user"]),
  userId: z.number().optional(),
  userIds: z.array(z.number()).optional(),
  roleName: z.string().optional(),
  branchId: z.number().optional(),
  title: z.string().min(1, "العنوان مطلوب"),
  message: z.string().min(1, "الرسالة مطلوبة"),
  category: z.string().optional(),
  priority: z.number(),
  actionUrl: z.string().optional(),
});

type FormValues = z.infer<typeof sendNotificationSchema>;

export function SendNotificationDialog({
  open,
  onOpenChange,
}: BaseOverlayProps) {
  const queryClient = useQueryClient();
  const [attachments, setAttachments] = useState<globalThis.File[]>([]);

  const form = useForm<FormValues>({
    resolver: zodResolver(sendNotificationSchema),
    defaultValues: {
      sendType: "user",
      priority: 1,
    },
  });

  const sendType = form.watch("sendType");

  const initialValues = useMemo<FormValues>(
    () => ({
      sendType: "user",
      userId: undefined,
      userIds: undefined,
      roleName: undefined,
      branchId: undefined,
      title: "",
      message: "",
      category: undefined,
      priority: 1,
      actionUrl: undefined,
    }),
    [],
  );

  const { data: users = [] } = useQuery<
    Array<{ userID: number; fullName: string }>
  >({
    queryKey: queryKeys.admin.usersAll,
    queryFn: async () => {
      const res = await api.get<{
        data: { items: Array<{ userID: number; fullName: string }> };
      }>("/users", { params: { pageSize: 1000 } });
      return res.data.data?.items || [];
    },
    enabled: !!open,
  });

  const { data: roles = [] } = useQuery<RoleOption[]>({
    queryKey: queryKeys.admin.adminRoles,
    queryFn: async () => {
      const res = await api.get<{ data: RoleOption[] } | RoleOption[]>(
        "/roles",
      );
      const data = res.data;
      return Array.isArray(data) ? data : data?.data || [];
    },
    enabled: !!open && (sendType === "role" || sendType === "multi-user"),
  });

  const { data: branches = [] } = useQuery<BranchOption[]>({
    queryKey: queryKeys.lookups.branches,
    queryFn: async () => {
      const res = await api.get<{ data: BranchOption[] } | BranchOption[]>(
        "/lookups/branches",
      );
      const data = res.data;
      return Array.isArray(data) ? data : data?.data || [];
    },
    enabled: !!open && sendType === "branch",
  });

  const handleFilesChange = (files: FileList | File | File[] | null) => {
    if (!files) return;
    let fileArray: File[];
    if (files instanceof FileList) {
      fileArray = Array.from(files);
    } else {
      fileArray = Array.isArray(files) ? files : [files];
    }
    setAttachments(prev => [...prev, ...fileArray]);
  };

  const handleFileRemove = (index: number) => {
    setAttachments((prev) => prev.filter((_, i) => i !== index));
  };

  const sendMutation = useMutation<ApiResponse<unknown>, Error, FormValues>({
    mutationFn: async (values) => {
      if (values.sendType === "user" || values.sendType === "multi-user") {
        const formData = new FormData();
        formData.append("title", values.title);
        formData.append("message", values.message);
        
        if (values.sendType === "user" && values.userId) {
          formData.append("userIds", values.userId.toString());
        } else if (values.sendType === "multi-user" && values.userIds) {
          values.userIds.forEach((id) => formData.append("userIds", id.toString()));
        }

        if (values.category) formData.append("category", values.category);
        if (values.priority) formData.append("priority", values.priority.toString());
        if (values.actionUrl) formData.append("actionUrl", values.actionUrl);

        attachments.forEach((file) => {
          formData.append("attachments", file);
        });

        return api
          .post<ApiResponse<unknown>>(
            "/admin/notifications/send-multi",
            formData,
            {
              headers: { "Content-Type": "multipart/form-data" },
            },
          )
          .then((r) => r.data);
      }
      const dto: AdminBroadcastNotificationDto = {
        title: values.title,
        message: values.message,
        category: values.category || undefined,
        priority: values.priority,
        actionUrl: values.actionUrl || undefined,
      };
      if (values.sendType === "role") {
        dto.roleName = values.roleName;
      } else {
        dto.branchID = values.branchId;
      }
      return notificationsApi.admin.broadcast(dto) as Promise<
        ApiResponse<unknown>
      >;
    },
    onSuccess: (result) => {
      if (result.isSuccess) {
        toast.success("تم إرسال الإشعار بنجاح");
        queryClient.invalidateQueries({ queryKey: ["admin-notifications"] });
        onOpenChange?.(false);
      } else {
        toast.error(result.message || "فشل إرسال الإشعار");
      }
    },
    onError: () => {
      toast.error("حدث خطأ أثناء إرسال الإشعار");
    },
  });

  const onSubmit = (values: FormValues) => {
    sendMutation.mutate(values);
  };

  return (
    <BaseFormDialog
      open={open}
      onOpenChange={onOpenChange}
      title="إرسال إشعار جديد"
      description="إرسال إشعار لمستخدم معين أو بث إشعار لجميع مستخدمي دور أو فرع"
      size="lg"
      isLoading={sendMutation.isPending}
      onSubmit={form.handleSubmit(onSubmit)}
      submitLabel="إرسال"
      submitIcon={<SendHorizonal className="w-4 h-4" />}
      resetOnOpen={false}
      reset={form.reset}
      initialValues={initialValues}
      showGlobalError
      formErrors={form.formState.errors}
      isDirty={form.formState.isDirty}
    >
      {/* Type Selection */}
      <FormSection title="نوع الإرسال" icon={SendHorizonal}>
        <div className="flex gap-2">
          {(["user", "multi-user", "role", "branch"] as const).map((type) => (
            <Button
              key={type}
              type="button"
              variant={sendType === type ? "primary" : "outline"}
              size="sm"
              onClick={() => {
                form.setValue("sendType", type);
                if (type !== "multi-user") {
                  setAttachments([]);
                }
              }}
              className="flex-1"
            >
              {type === "user" ? (
                <User className="w-4 h-4" />
              ) : type === "multi-user" ? (
                <Users className="w-4 h-4" />
              ) : type === "role" ? (
                <Users className="w-4 h-4" />
              ) : (
                <Building2 className="w-4 h-4" />
              )}
              {type === "user"
                ? "مستخدم"
                : type === "multi-user"
                  ? "متعدد"
                  : type === "role"
                    ? "دور"
                    : "فرع"}
            </Button>
          ))}
        </div>
      </FormSection>

      {/* Recipient Selection */}
      {sendType === "user" && (
        <FormSection title="المستلم" icon={User}>
          <FormField
            label="المستخدم"
            required
            error={form.formState.errors.userId?.message}
          >
            <Controller
              name="userId"
              control={form.control}
              render={({ field }) => (
                <NativeSelect
                  value={field.value ?? ""}
                  onChange={(val) =>
                    field.onChange(val ? Number(val) : undefined)
                  }
                  options={users.map((u) => ({
                    label: u.fullName,
                    value: String(u.userID),
                  }))}
                  placeholder="اختر المستخدم..."
                  icon={User}
                />
              )}
            />
          </FormField>
        </FormSection>
      )}

      {sendType === "role" && (
        <FormSection title="الدور المستهدف" icon={Users}>
          <FormField
            label="الدور"
            required
            error={form.formState.errors.roleName?.message}
          >
            <Controller
              name="roleName"
              control={form.control}
              render={({ field }) => (
                <NativeSelect
                  value={field.value ?? ""}
                  onChange={(val) => field.onChange(val || undefined)}
                  options={roles.map((r) => ({
                    label: r.roleName,
                    value: r.roleName,
                  }))}
                  placeholder="اختر الدور..."
                  icon={Users}
                />
              )}
            />
          </FormField>
        </FormSection>
      )}

      {sendType === "branch" && (
        <FormSection title="الفرع المستهدف" icon={Building2}>
          <FormField
            label="الفرع"
            required
            error={form.formState.errors.branchId?.message}
          >
            <Controller
              name="branchId"
              control={form.control}
              render={({ field }) => (
                <NativeSelect
                  value={field.value ?? ""}
                  onChange={(val) =>
                    field.onChange(val ? Number(val) : undefined)
                  }
                  options={branches.map((b) => ({
                    label: b.name,
                    value: String(b.id),
                  }))}
                  placeholder="اختر الفرع..."
                  icon={Building2}
                />
              )}
            />
          </FormField>
        </FormSection>
      )}

      {sendType === "multi-user" && (
        <FormSection title="المستخدمون المستهدفون" icon={Users}>
          <FormField
            label="اختر المستخدمين"
            required
            error={form.formState.errors.userIds?.message}
          >
            <Controller
              name="userIds"
              control={form.control}
              render={({ field }) => (
                <div className="max-h-60 overflow-y-auto border border-border rounded-lg p-2">
                  {users.map((u) => (
                    <label
                      key={u.userID}
                      className="flex items-center gap-2 p-2 hover:bg-muted/50 rounded transition-colors cursor-pointer"
                    >
                      <input
                        type="checkbox"
                        value={u.userID}
                        checked={field.value?.includes(u.userID)}
                        onChange={(e) => {
                          const newIds = field.value ? [...field.value] : [];
                          if (e.target.checked) {
                            if (!newIds.includes(u.userID))
                              newIds.push(u.userID);
                          } else {
                            const idx = newIds.indexOf(u.userID);
                            if (idx > -1) newIds.splice(idx, 1);
                          }
                          field.onChange(newIds);
                          field.onBlur();
                        }}
                        className="w-4 h-4 accent-primary"
                      />
                      <span className="text-sm">{u.fullName}</span>
                    </label>
                  ))}
                </div>
              )}
            />
          </FormField>
        </FormSection>
      )}

      {/* Attachments Section */}
      {(sendType === "multi-user" || sendType === "user") && (
        <FormSection title="المرفقات" icon={Paperclip}>
          <div className="space-y-3">
            <label className="flex flex-col items-center justify-center w-full p-6 border-2 border-dashed border-border rounded-lg cursor-pointer hover:border-primary hover:bg-primary/5 transition-colors">
              <Upload className="w-8 h-8 text-muted-foreground mb-2" />
              <Text variant="description" className="text-center">
                اسحب الملفات هنا أو انقر للاختيار
              </Text>
              <Text variant="caption" className="text-muted-foreground mt-1">
                يدعم: PDF, DOC, DOCX, XLS, XLSX, Images (أقصى 10 ميجابايت لكل
                ملف)
              </Text>
              <input
                type="file"
                accept="application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,image/*"
                multiple
                onChange={(e) => handleFilesChange(e.target.files)}
                className="sr-only"
                id="notification-attachments"
              />
            </label>
            {attachments.length > 0 && (
              <div className="mt-3 space-y-1">
                <Text variant="caption" className="font-medium">
                  الملفات المرفقة ({attachments.length})
                </Text>
                <div className="flex flex-wrap gap-2">
                  {attachments.map((file, index) => (
                    <div
                      key={index}
                      className="flex items-center gap-2 px-3 py-2 bg-muted/50 border border-border rounded-lg text-sm"
                    >
                      <FileIcon className="w-4 h-4 text-muted-foreground" />
                      <span className="truncate max-w-[200px] sm:max-w-[300px] font-medium">
                        {file.name}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        ({(file.size / 1024).toFixed(1)} KB)
                      </span>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6 text-destructive hover:bg-destructive/10"
                        onClick={() => handleFileRemove(index)}
                      >
                        <X className="w-3 h-3" />
                      </Button>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </FormSection>
      )}

      {/* Content */}
      <FormSection title="محتوى الإشعار" icon={SendHorizonal}>
        <FormField
          label="العنوان"
          required
          error={form.formState.errors.title?.message}
        >
          <Input {...form.register("title")} placeholder="عنوان الإشعار" />
        </FormField>

        <FormField
          label="الرسالة"
          required
          error={form.formState.errors.message?.message}
        >
          <Textarea
            {...form.register("message")}
            placeholder="نص الإشعار"
            rows={3}
          />
        </FormField>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField label="التصنيف">
            <Controller
              name="category"
              control={form.control}
              render={({ field }) => (
                <NativeSelect
                  value={field.value ?? ""}
                  onChange={(val) => field.onChange(val || undefined)}
                  options={Object.entries(CATEGORY_LABELS).map(
                    ([key, label]) => ({ label, value: key }),
                  )}
                  placeholder="اختر التصنيف..."
                />
              )}
            />
          </FormField>

          <FormField label="الأولوية">
            <Controller
              name="priority"
              control={form.control}
              render={({ field }) => (
                <NativeSelect
                  value={field.value}
                  onChange={(val) => field.onChange(Number(val))}
                  options={[
                    { label: "عادية", value: "1" },
                    { label: "متوسطة", value: "2" },
                    { label: "عالية", value: "3" },
                  ]}
                />
              )}
            />
          </FormField>
        </div>

        <FormField
          label="رابط الإجراء"
          hint="رابط اختياري للصفحة المرتبطة (مثال: /retirement-files/5)"
        >
          <Input {...form.register("actionUrl")} placeholder="/مسار/الصفحة" />
        </FormField>
      </FormSection>
    </BaseFormDialog>
  );
}
