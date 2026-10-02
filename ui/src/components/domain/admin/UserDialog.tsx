import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Activity,Building2, Key, ShieldCheck, User as UserIcon } from "lucide-react";
import { useMemo, useState } from "react";
import { Controller, type Resolver,useForm } from "react-hook-form";
import * as z from "zod";

import api from "@/api/axios";
import { usersApi } from "@/api/users";
import {
    BaseFormDialog,
    type BaseOverlayProps,
    Checkbox,    FormField,
    FormSection,
    Input,
    NativeSelect,
    NoteBox} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const createUserSchema = z.object({
    fullName: z.string().trim().min(3, "الاسم الكامل يجب أن يكون 3 أحرف على الأقل"),
    username: z.string().trim().min(3, "اسم المستخدم يجب أن يكون 3 أحرف على الأقل").regex(/^[a-zA-Z0-9._-]+$/, "اسم المستخدم يجب أن يحتوي على أحرف وأرقام فقط"),
    email: z.string().trim().email("بريد إلكتروني غير صالح").or(z.literal("")),
    password: z.string().min(8, "كلمة المرور يجب أن تكون 8 أحرف على الأقل"),
    roleID: z.coerce.number().min(1, "يجب اختيار الدور"),
    branchID: z.coerce.number().min(1, "يجب اختيار الفرع"),
    status: z.enum(["Active", "Locked", "Suspended"]),
});

const editUserSchema = z.object({
    fullName: z.string().trim().min(3, "الاسم الكامل يجب أن يكون 3 أحرف على الأقل"),
    username: z.string().trim().min(3, "اسم المستخدم يجب أن يكون 3 أحرف على الأقل").regex(/^[a-zA-Z0-9._-]+$/, "اسم المستخدم يجب أن يحتوي على أحرف وأرقام فقط"),
    email: z.string().trim().email("بريد إلكتروني غير صالح").or(z.literal("")),
    password: z.string(),
    roleID: z.coerce.number().min(1, "يجب اختيار الدور"),
    branchID: z.coerce.number().min(1, "يجب اختيار الفرع"),
    status: z.enum(["Active", "Locked", "Suspended"]),
    changePassword: z.boolean(),
}).superRefine((data, ctx) => {
    if (data.changePassword && (!data.password || data.password.length < 8)) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "كلمة المرور يجب أن تكون 8 أحرف على الأقل",
            path: ["password"],
        });
    }
});

type CreateUserFormValues = z.infer<typeof createUserSchema>;
type EditUserFormValues = z.infer<typeof editUserSchema>;
type UserFormValues = CreateUserFormValues | EditUserFormValues;

// ─── Types ─────────────────────────────────────────────

interface RoleOption { roleID: number; roleName: string; }
interface BranchOption { id: number; name: string; }

interface UserData {
    userID: number;
    fullName: string;
    username: string;
    email: string | null;
    roleID: number | null;
    branchID: number | null;
    status?: "Active" | "Locked" | "Suspended";
    isActive: boolean;
}

interface Props extends BaseOverlayProps {
    onSaved: () => void;
    initialData?: UserData | null;
}

/**
 * Standardized UserDialog.
 * Refactored to strictly use BaseFormDialog, FormSection, and resetOnOpen.
 */
export function UserDialog({
    open,
    onOpenChange,
    onSaved,
    initialData
}: Props) {
    const user = initialData;
    const isEditing = !!user;
    const [changePassword, setChangePassword] = useState(false);

    const mutation = useApiMutation(
        (values: UserFormValues) => {
            const payload = {
                fullName: values.fullName,
                username: values.username,
                email: values.email,
                roleID: Number(values.roleID),
                branchID: Number(values.branchID),
                isActive: values.status === "Active",
                status: values.status,
                ...(isEditing
                    ? (values as EditUserFormValues).changePassword && values.password
                        ? { password: values.password }
                        : {}
                    : { password: values.password }),
            };
            return isEditing
                ? usersApi.update(user!.userID, payload)
                : usersApi.create(payload);
        },
        {
            successMessage: isEditing ? "تم تحديث بيانات المستخدم بنجاح" : "تم إنشاء المستخدم الجديد بنجاح",
            onSuccessCallback: () => {
                onSaved();
                setChangePassword(false);
                onOpenChange?.(false);
            }
        }
    );

    // Fetch Reference Data
    const { data: roles = [] } = useQuery<RoleOption[]>({
        queryKey: ["roles"],
        queryFn: async () => {
            const res = await api.get<{ data: RoleOption[] } | RoleOption[]>("/roles");
            const data = res.data;
            return Array.isArray(data) ? data : (data?.data || []);
        },
        enabled: !!open
    });

    const { data: branches = [] } = useQuery<BranchOption[]>({
        queryKey: ["lookups", "branches"],
        queryFn: async () => {
            const res = await api.get<{ data: BranchOption[] } | BranchOption[]>("/lookups/branches");
            const data = res.data;
            return Array.isArray(data) ? data : (data?.data || []);
        },
        enabled: !!open
    });

    const form = useForm<UserFormValues>({
        resolver: zodResolver(isEditing ? editUserSchema : createUserSchema) as Resolver<UserFormValues>,
        defaultValues: {
            fullName: "",
            username: "",
            email: "",
            password: "",
            roleID: 0,
            branchID: 0,
            status: "Active",
            ...(isEditing ? { changePassword: false } : {}),
        },
    });

    const { register, handleSubmit, reset, control, formState: { errors, isDirty } } = form;

    const onSubmit = (values: UserFormValues) => {
        mutation.mutate(values);
    };

    const initialValues = useMemo(() => {
        if (!user) return undefined;
        return {
            fullName: user.fullName,
            username: user.username,
            email: user.email || "",
            roleID: user.roleID || 0,
            branchID: user.branchID || 0,
            status: user.status || (user.isActive ? "Active" : "Suspended"),
            password: "",
            ...(isEditing ? { changePassword: false } : {}),
        };
    }, [user, isEditing]);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={isEditing ? "تعديل حساب مستخدم" : "إضافة مستخدم جديد"}
            description="إدارة صلاحيات الوصول والبيانات الأمنية للحساب"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={handleSubmit(onSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={isEditing ? "حفظ التعديلات" : "إنشاء الحساب"}
        >
            <div className="space-y-4">
                <FormSection title="البيانات الشخصية والاعتماد" icon={UserIcon}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الاسم الكامل" required error={errors.fullName?.message}>
                            <Input placeholder="الاسم الثلاثي واللقب" {...register("fullName")} />
                        </FormField>

                        <FormField label="اسم المستخدم" required error={errors.username?.message}>
                            <Input
                                placeholder="اسم المستخدم"
                                {...register("username")}
                                disabled={isEditing}
                                className="font-mono"
                            />
                        </FormField>

                        <FormField label="البريد الإلكتروني" error={errors.email?.message}>
                            <Input placeholder="name@domain.com" {...register("email")} className="font-mono" />
                        </FormField>

                        {isEditing ? (
                            <div className="space-y-2">
                                <Controller
                                    name="changePassword"
                                    control={control}
                                    render={({ field }) => (
                                        <Checkbox
                                            id="changePassword"
                                            checked={field.value}
                                            onCheckedChange={(checked) => {
                                                field.onChange(checked);
                                                setChangePassword(!!checked);
                                            }}
                                            label="تغيير كلمة المرور"
                                        />
                                    )}
                                />
                                {changePassword && (
                                    <FormField label="كلمة المرور الجديدة" required error={errors.password?.message}>
                                        <Input type="password" placeholder="8 خانات على الأقل" {...register("password")} icon={Key} />
                                    </FormField>
                                )}
                            </div>
                        ) : (
                            <FormField label="كلمة المرور" required error={errors.password?.message}>
                                <Input type="password" placeholder="8 خانات على الأقل" {...register("password")} icon={Key} />
                            </FormField>
                        )}
                    </div>
                </FormSection>

                <FormSection title="الصلاحيات والنطاق الإداري" icon={ShieldCheck}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الدور / مجموعة الصلاحيات" required error={errors.roleID?.message}>
                            <Controller
                                name="roleID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={roles.map((r: RoleOption) => ({ label: r.roleName, value: String(r.roleID) }))}
                                        value={String(field.value)}
                                        onChange={(val) => field.onChange(Number(val))}
                                        placeholder="اختر الدور..."
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label="الفرع المرتبط" required error={errors.branchID?.message}>
                            <Controller
                                name="branchID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={branches.map((b: BranchOption) => ({ label: b.name, value: String(b.id) }))}
                                        value={String(field.value)}
                                        onChange={(val) => field.onChange(Number(val))}
                                        placeholder="اختر الفرع..."
                                        icon={Building2}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <FormSection title="حالة الحساب" icon={Activity}>
                    <div className="max-w-md">
                        <FormField label="حالة الدخول" required error={errors.status?.message}>
                            <Controller
                                name="status"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect
                                        options={[
                                            { label: "نشط (قيد العمل)", value: "Active" },
                                            { label: "مغلق (Locked)", value: "Locked" },
                                            { label: "معلق (Suspended)", value: "Suspended" }
                                        ]}
                                        value={field.value}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>

                <NoteBox type="info" title="تنبيه أمني">
                    الحسابات المغلقة تمنع الدخول حتى يتم تصفير محاولات الدخول الخاطئة من قبل المدير.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
