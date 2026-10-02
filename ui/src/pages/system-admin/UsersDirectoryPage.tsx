import { useQuery } from "@tanstack/react-query";
import {
Building2, ChevronDown, ChevronUp, Pencil, RotateCcw,
    Shield, ShieldCheck,
Trash2,     UserCog,     Users, X} from "lucide-react";
import { useEffect,useState } from "react";
import { toast } from "sonner";

import { usersApi } from "@/api/users";
import { categoryColors, categoryLabels,getPermissionName } from "@/components/domain/admin/permission-utils";
import { RolePermissionsDialog } from "@/components/domain/admin/RolePermissionsDialog";
import { UserDialog } from "@/components/domain/admin/UserDialog";
import { UserPermissionsDialog } from "@/components/domain/admin/UserPermissionsDialog";
import { Badge, Button, Card, CrudPageLayout, Heading, Text, useConfirm } from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation,useCrudState } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { kpiToneClass } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface AssignmentItem {
    id: number;
    roleId: number;
    roleName: string;
    branchId: number | null;
    branchName: string | null;
    isActive: boolean;
    createdAt: string;
}

interface UserDto {
    userID: number;
    fullName: string;
    username: string;
    email: string | null;
    roleID: number | null;
    roleName: string | null;
    branchID: number | null;
    branchName: string | null;
    isActive: boolean;
    isLocked: boolean;
    lastLogin: string | null;
    createdAt: string;
    permissions: string[];
    assignments: AssignmentItem[];
}

// ─── Page ──────────────────────────────────────────────

export function UsersDirectoryPage() {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const [expandedUser, setExpandedUser] = useState<UserDto | null>(null);
    const [page, setPage] = useState(1);
    const pageSize = 20;

    // useCrudState for main user modal
    const crud = useCrudState<UserDto>();

    // Role permissions modal
    const [permModalOpen, setPermModalOpen] = useState(false);
    const [permRoleId, setPermRoleId] = useState<number | null>(null);
    const [permRoleName, setPermRoleName] = useState("");

    // ═══ User-level permissions modal ═══
    const [userPermModalOpen, setUserPermModalOpen] = useState(false);
    const [userPermTarget, setUserPermTarget] = useState<UserDto | null>(null);

    // ─── Data Fetching ──────────────────────────────────

    const { data: usersPaged, isLoading: usersLoading, error: usersError, refetch: refetchUsers } = useQuery({
        queryKey: queryKeys.admin.users.list({ page, pageSize }),
        queryFn: () => usersApi.getAll({ page, pageSize })
    });

    const users = usersPaged?.items || [];
    const totalCount = usersPaged?.totalCount || 0;

    // Auto-navigate back if the current page becomes empty (e.g., after delete)
    useEffect(() => {
        if (usersPaged && users.length === 0 && page > 1) {
            setPage(p => p - 1);
        }
    }, [usersPaged, users.length, page]);

    const { data: roles = [], isLoading: rolesLoading, error: rolesError } = useQuery({
        queryKey: queryKeys.admin.roles,
        queryFn: () => usersApi.getRoles()
    });

    useEffect(() => {
        if (usersError) {
            toast.error("فشل تحميل المستخدمين", {
                description: normalizeApiError(usersError).message
            });
        }
    }, [usersError]);

    useEffect(() => {
        if (rolesError) {
            toast.error("فشل تحميل الأدوار", {
                description: normalizeApiError(rolesError).message
            });
        }
    }, [rolesError]);

    const loading = usersLoading || rolesLoading;

    // ─── Mutations ──────────────────────────────────────

    const deleteMutation = useApiMutation(
        async (userId: number) => usersApi.delete(userId),
        {
            successMessage: "تم حذف المستخدم بنجاح",
            invalidateKeys: [queryKeys.admin.users.all],
        }
    );

    const resetPasswordMutation = useApiMutation(
        async ({ userId, newPass }: { userId: number; newPass: string }) =>
            usersApi.resetPassword(userId, newPass),
        {
            successMessage: "تم إعادة تعيين كلمة المرور بنجاح",
        }
    );

    // ─── Handlers ───────────────────────────────────────

    const handleDelete = async (user: UserDto) => {
        const confirmed = await confirm({
            title: "تأكيد الحذف",
            description: `هل أنت متأكد من حذف "${user.fullName}"؟`,
            variant: "danger",
        });
        if (!confirmed) return;
        deleteMutation.mutate(user.userID);
    };

    const handleResetPassword = async (user: UserDto) => {
        const newPassword = "12345";
        const confirmed = await confirm({
            title: "إعادة تعيين كلمة المرور",
            description: `سيتم إعادة تعيين كلمة المرور للمستخدم "${user.fullName}".\n\nكلمة المرور الجديدة: ${newPassword}\n\nسيُطلب من المستخدم تغييرها عند تسجيل الدخول.`,
        });
        if (!confirmed) return;
        resetPasswordMutation.mutate({ userId: user.userID, newPass: newPassword });
    };

    const openPermissionsModal = (roleId: number, roleName: string) => {
        setPermRoleId(roleId);
        setPermRoleName(roleName);
        setPermModalOpen(true);
    };

    const openUserPermModal = (user: UserDto) => {
        setUserPermTarget(user);
        setUserPermModalOpen(true);
    };

    // ─── Column Definitions ─────────────────────────────

    const columns = [
        {
            key: "roleName",
            header: "الدور",
            render: (row: UserDto) => (
                <Button
                    variant="ghost"
                    className="justify-start gap-2 px-3 py-2 h-auto -mx-2"
                    onClick={(e) => {
                        e.stopPropagation();
                        if (row.roleID) openPermissionsModal(row.roleID, row.roleName || "");
                    }}
                    title="اضغط لإدارة صلاحيات الدور"
                >
                    <ShieldCheck className="w-4 h-4 text-muted-primary" />
                    <span className="text-sm font-medium">{row.roleName || "—"}</span>
                </Button>
            ),
        },
        {
            key: "fullName",
            header: "المستخدم",
            render: (row: UserDto) => (
                <div className="flex items-center gap-3">
                    <div className={`flex items-center justify-center w-9 h-9 rounded-full ${row.isActive
                        ? "bg-gradient-to-br from-primary/20 to-primary/5"
                        : "bg-muted"
                        }`}>
                        <Users className={`w-4 h-4 ${row.isActive ? "text-primary" : "text-muted-foreground"}`} />
                    </div>
                    <div>
                        <p className="font-semibold text-foreground">{row.fullName}</p>
                        <Text variant="caption" className="font-mono" dir="ltr">{row.username}</Text>
                    </div>
                </div>
            ),
        },
        {
            key: "branchName",
            header: "الفرع",
            render: (row: UserDto) => (
                <div className="flex items-center gap-1.5">
                    {row.branchName && <Building2 className="w-3.5 h-3.5 text-muted-foreground" />}
                    <span className="text-sm">{row.branchName || "مركزي"}</span>
                </div>
            ),
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: UserDto) => (
                <Badge variant={row.isActive ? "success" : "danger"} dot>
                    {row.isActive ? "نشط" : "معطل"}
                </Badge>
            ),
        },
        {
            key: "lastLogin",
            header: "آخر دخول",
            render: (row: UserDto) => (
                <span className="text-xs text-muted-foreground">
                    {row.lastLogin
                        ? new Date(row.lastLogin).toLocaleDateString("ar-SA")
                        : "لم يسجل"}
                </span>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: UserDto) => (
                <div className="flex items-center gap-0.5">
                    <Button variant="ghost" size="icon"
                        onClick={(e) => { e.stopPropagation(); setExpandedUser(expandedUser?.userID === row.userID ? null : row); }}
                        title="تفاصيل الصلاحيات"
                        aria-label="تفاصيل الصلاحيات"
                    >
                        {expandedUser?.userID === row.userID ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
                    </Button>
                    <Button variant="ghost" size="icon"
                        onClick={(e) => { e.stopPropagation(); openUserPermModal(row); }}
                        title="صلاحيات فردية"
                        aria-label="صلاحيات فردية"
                        className="text-amber-600 hover:text-amber-700"
                    >
                        <UserCog className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={(e) => { e.stopPropagation(); crud.openEdit(row); }} title="تعديل" aria-label="تعديل">
                        <Pencil className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={(e) => { e.stopPropagation(); handleResetPassword(row); }} title="إعادة كلمة المرور" aria-label="إعادة تعيين كلمة المرور">
                        <RotateCcw className="w-4 h-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={(e) => { e.stopPropagation(); handleDelete(row); }} title="حذف" aria-label="حذف المستخدم">
                        <Trash2 className="w-4 h-4 text-destructive" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="إدارة المستخدمين والصلاحيات"
            description="إدارة الحسابات وتعيين الأدوار والصلاحيات الدقيقة"
            createLabel="إضافة مستخدم"
            onCreateClick={crud.openCreate}
            columns={columns}
            data={users}
            loading={loading}
            rowKey={(row) => row.userID}
            pagination={{
                page,
                pageSize,
                total: totalCount,
                onPageChange: (p) => setPage(p)
            }}
        >
            {/* Stats */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
                {[
                    { label: "إجمالي المستخدمين", value: totalCount, color: "primary" },
                    { label: "الأدوار", value: roles.length, color: "special" },
                ].map((stat) => (
                    <Card key={stat.label} className="p-4">
                        <Text variant="caption">{stat.label}</Text>
                        <Text variant="body" className={`md:text-xl lg:text-2xl font-bold ${kpiToneClass(stat.color)}`}>{stat.value}</Text>
                    </Card>
                ))}
            </div>

            {/* Expanded User Details Panel */}
            {expandedUser && (
                <Card className="p-5 animate-fade-in mb-6">
                    <div className="flex items-center justify-between mb-4 flex-wrap">
                        <Heading level="h3" className="font-semibold text-foreground flex items-center gap-2">
                            <Shield className="w-4 h-4 text-primary" />
                            صلاحيات &quot;{expandedUser.fullName}&quot;
                        </Heading>
                        <div className="flex items-center gap-2">
                            <Button variant="outline" size="sm" onClick={() => openUserPermModal(expandedUser)}>
                                <UserCog className="w-3.5 h-3.5" />
                                إدارة صلاحيات فردية
                            </Button>
                            <Button variant="ghost" size="icon" onClick={() => setExpandedUser(null)} aria-label="إغلاق">
                                <X className="w-4 h-4" />
                            </Button>
                        </div>
                    </div>
                    <div className="space-y-4">
                        {expandedUser.assignments.length > 0 && (
                            <div>
                                <Text variant="caption" className="font-semibold mb-2">تعيينات الأدوار النشطة</Text>
                                <div className="flex flex-wrap gap-2">
                                    {expandedUser.assignments.map(a => (
                                        <div key={a.id} className="flex items-center gap-2 rounded-lg bg-muted/30 border px-3 py-1.5 text-sm">
                                            <ShieldCheck className="w-3.5 h-3.5 text-primary" />
                                            <span className="font-medium">{a.roleName}</span>
                                            {a.branchName && (
                                                <span className="text-xs text-muted-foreground">({a.branchName})</span>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        <div>
                            <Text variant="caption" className="font-semibold mb-2">
                                الصلاحيات الفعلية ({expandedUser.permissions.length})
                            </Text>
                            {expandedUser.permissions.length > 0 ? (
                                <div className="flex flex-wrap gap-1.5">
                                    {expandedUser.permissions.filter(Boolean).map(code => {
                                        const cat = code.split(".")[0];
                                        return (
                                            <span key={code}
                                                className={`inline-flex items-center gap-1 rounded-md px-2 py-0.5 text-xs font-medium ${categoryColors[cat] || categoryColors["System"]
                                                    }`}
                                                title={code}
                                            >
                                                {categoryLabels[cat] || cat} - {getPermissionName(code)}
                                            </span>
                                        );
                                    })}
                                </div>
                            ) : (
                                <Text variant="caption" className="italic">
                                    لا توجد صلاحيات — يرجى تعيين دور أو التحقق من RolePermissions
                                </Text>
                            )}
                        </div>
                    </div>
                </Card>
            )}

            <UserDialog
                open={crud.isFormOpen}
                onOpenChange={crud.isFormOpen ? crud.closeForm : crud.openCreate}
                initialData={crud.selectedItem}
                onSaved={refetchUsers}
            />

            <RolePermissionsDialog
                open={permModalOpen}
                onOpenChange={setPermModalOpen}
                roleId={permRoleId}
                roleName={permRoleName}
                onSaved={refetchUsers}
            />

            <UserPermissionsDialog
                open={userPermModalOpen}
                onOpenChange={setUserPermModalOpen}
                user={userPermTarget}
                onSaved={refetchUsers}
            />
            {ConfirmDialogElement}
        </CrudPageLayout>
    );
}
