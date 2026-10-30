import { useQuery } from "@tanstack/react-query";
import { Search, ShieldMinus,ShieldPlus } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import { usersApi } from "@/api/users";
import { Badge, BaseFormDialog, Button, Input, Text } from "@/components/ui";
<<<<<<< HEAD
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
=======
import { useApiMutation } from "@/hooks";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

import { categoryColors, categoryLabels,getPermissionName } from "./permission-utils";

// ─── Types ─────────────────────────────────────────────

interface PermissionItem {
    permissionId: number;
    code: string;
    description: string | null;
    category: string | null;
}

interface UserDto {
    userID: number;
    fullName: string;
}

type UserPermState = "grant" | "revoke" | "none";

interface UserPermEntry {
    permissionId: number;
    state: UserPermState;
}

interface UserPermissionsDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    user: UserDto | null;
    onSaved: () => void;
}

export function UserPermissionsDialog({
    open,
    onOpenChange,
    user,
    onSaved
}: UserPermissionsDialogProps) {
    const [userPermEntries, setUserPermEntries] = useState<UserPermEntry[]>([]);
    const [searchTerm, setSearchTerm] = useState("");

    const { data: allPermissions = [], isLoading: loadingAll } = useQuery({
        queryKey: queryKeys.admin.permissions,
        queryFn: () => usersApi.getPermissions(),
        enabled: open
    });

    const { data: userPermsData = [], isLoading: loadingUserPerms } = useQuery<{ permissionId: number; isGranted: boolean }[]>({
        queryKey: ["users", user?.userID, "individual-permissions"],
        queryFn: async () => {
            if (!user) return [];
            return usersApi.getUserPermissions(user.userID);
        },
        enabled: open && !!user && allPermissions.length > 0,
    });

    useEffect(() => {
        if (allPermissions.length > 0 && (userPermsData.length > 0 || (open && user && !loadingUserPerms))) {
            const currentEntries: UserPermEntry[] = allPermissions.map(p => {
                const existing = userPermsData.find((up: { permissionId: number; isGranted: boolean }) => up.permissionId === p.permissionId);
                return {
                    permissionId: p.permissionId,
                    state: existing ? (existing.isGranted ? "grant" : "revoke") : "none",
                };
            });
            setUserPermEntries(currentEntries);
        }
    }, [userPermsData, allPermissions, open, user, loadingUserPerms]);

    const loading = loadingAll || loadingUserPerms;

    const saveMutation = useApiMutation(
        (permissions: { permissionId: number; isGranted: boolean }[]) => 
            usersApi.updateUserPermissions(user!.userID, permissions),
        {
            successMessage: `تم تحديث صلاحيات "${user?.fullName}" الفردية`,
            onSuccess: () => {
                onOpenChange(false);
                onSaved();
            }
        }
    );

    const cycleUserPerm = (pid: number) => {
        setUserPermEntries(prev =>
            prev.map(e => {
                if (e.permissionId !== pid) return e;
                const next: UserPermState = e.state === "none" ? "grant" : e.state === "grant" ? "revoke" : "none";
                return { ...e, state: next };
            })
        );
    };

    const handleSave = () => {
        if (!user) return;
        const permissions = userPermEntries
            .filter(e => e.state !== "none")
            .map(e => ({ permissionId: e.permissionId, isGranted: e.state === "grant" }));
        saveMutation.mutate(permissions);
    };

    const groupByCategory = (perms: PermissionItem[]) => {
        const groups: Record<string, PermissionItem[]> = {};
        perms.forEach(p => {
            const cat = p.category || "أخرى";
            if (!groups[cat]) groups[cat] = [];
            groups[cat].push(p);
        });
        return groups;
    };

    const PermStateIcon = ({ state }: { state: UserPermState }) => {
        if (state === "grant") return <ShieldPlus className="w-4 h-4 text-emerald-600" />;
        if (state === "revoke") return <ShieldMinus className="w-4 h-4 text-red-500" />;
        return <div className="w-4 h-4 rounded border-2 border-border dark:border-gray-600" />;
    };

    const filteredPermissions = useMemo(() => 
        allPermissions.filter(p => 
            p.code.toLowerCase().includes(searchTerm.toLowerCase()) || 
            p.description?.toLowerCase().includes(searchTerm.toLowerCase())
        ),
        [allPermissions, searchTerm]
    );

    const groupedPermissions = useMemo(() => 
        groupByCategory(filteredPermissions),
        [filteredPermissions]
    );

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={`صلاحيات فردية — "${user?.fullName}"`}
            description="أضف صلاحيات إضافية أو اسحب صلاحيات محددة لهذا المستخدم فقط — مستقلة عن دوره"
            size="5xl"
            isLoading={saveMutation.isPending}
            onSubmit={handleSave}
            submitLabel="حفظ الصلاحيات الفردية"
        >
            <div className="flex items-center gap-4 p-3 rounded-lg bg-muted/30 border text-xs">
                <div className="flex items-center gap-1.5">
                    <div className="w-4 h-4 rounded border-2 border-border" />
                    <span>بدون تعديل</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <ShieldPlus className="w-4 h-4 text-emerald-600" />
                    <span className="text-emerald-600 font-medium">منح فردي</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <ShieldMinus className="w-4 h-4 text-red-500" />
                    <span className="text-red-500 font-medium">سحب فردي</span>
                </div>
                <div className="me-auto text-muted-foreground italic">اضغط لتدوير الحالة</div>
            </div>

            <div className="space-y-4 mt-2 max-h-[55dvh] overflow-y-auto pe-2">
                {loading ? (
                    <div className="flex justify-center py-10 text-muted-foreground italic">جاري تحميل الصلاحيات...</div>
                ) : (
                    <>
                        <div className="sticky top-0 z-10 bg-background pb-2">
                            <div className="relative">
                                <Search className="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                                <Input 
                                    placeholder="بحث عن صلاحية في القائمة..." 
                                    className="ps-9"
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                />
                            </div>
                        </div>

                        {Object.entries(groupedPermissions).map(([category, perms]) => {
                            const filteredPermIds = perms.map(p => p.permissionId);
                            const modified = perms.filter(p => {
                                const entry = userPermEntries.find(e => e.permissionId === p.permissionId);
                                return entry && entry.state !== "none";
                            }).length;

                            return (
                                <div key={category} className="space-y-2 border-b pb-4 last:border-0">
                                    <div className="flex items-center justify-between bg-muted/20 p-2 rounded-md">
                                        <div className="flex items-center gap-2">
                                            <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-semibold ${categoryColors[category] || "bg-muted text-muted-foreground"}`}>
                                                {categoryLabels[category] || category}
                                            </span>
                                            {modified > 0 && (
                                                <Badge variant="warning" className="text-[10px] sm:text-xs">{modified} تعديل</Badge>
                                            )}
                                        </div>
                                        <div className="flex gap-1">
                                            <Button variant="ghost" size="sm" className="h-7 text-[9px] text-emerald-600"
                                                onClick={() => {
                                                    setUserPermEntries(prev => {
                                                        const newEntries = [...prev];
                                                        filteredPermIds.forEach(id => {
                                                            const idx = newEntries.findIndex(e => e.permissionId === id);
                                                            if (idx > -1) newEntries[idx].state = "grant";
                                                            else newEntries.push({ permissionId: id, state: "grant" });
                                                        });
                                                        return newEntries;
                                                    });
                                                }}
                                            >منح الكل</Button>
                                            <Button variant="ghost" size="sm" className="h-7 text-[9px] text-red-500"
                                                onClick={() => {
                                                    setUserPermEntries(prev => {
                                                        const newEntries = [...prev];
                                                        filteredPermIds.forEach(id => {
                                                            const idx = newEntries.findIndex(e => e.permissionId === id);
                                                            if (idx > -1) newEntries[idx].state = "revoke";
                                                            else newEntries.push({ permissionId: id, state: "revoke" });
                                                        });
                                                        return newEntries;
                                                    });
                                                }}
                                            >سحب الكل</Button>
                                            <Button variant="ghost" size="sm" className="h-7 text-[9px]"
                                                onClick={() => {
                                                    setUserPermEntries(prev => prev.filter(e => !filteredPermIds.includes(e.permissionId)));
                                                }}
                                            >إعادة تعيين</Button>
                                        </div>
                                    </div>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
                                        {perms.map(p => {
                                            const entry = userPermEntries.find(e => e.permissionId === p.permissionId);
                                            const state = entry?.state || "none";

                                            return (
                                                <Button
                                                    key={p.permissionId}
                                                    type="button"
                                                    variant="outline"
                                                    className="justify-start gap-3 h-auto p-3"
                                                    onClick={() => cycleUserPerm(p.permissionId)}
                                                >
                                                    <PermStateIcon state={state} />
                                                    <div className="flex-1 min-w-0 text-start">
                                                        <Text variant="label" className="leading-tight">{getPermissionName(p.code)}</Text>
<<<<<<< HEAD
                                                        <p className="text-[10px] sm:text-xs text-muted-foreground font-mono mt-0.5">{p.code}</p>
=======
                                                        <p className="text-[10px] sm:text-xs text-muted-foreground/70 font-mono mt-0.5">{p.code}</p>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                        {p.description && (
                                                            <p className="text-[11px] text-muted-foreground truncate mt-1">{p.description}</p>
                                                        )}
                                                    </div>
                                                    {state !== "none" && (
                                                        <span className={`text-[10px] sm:text-xs font-medium shrink-0 ${state === "grant" ? "text-emerald-600" : "text-red-500"
                                                            }`}>
                                                            {state === "grant" ? "ممنوحة" : "مسحوبة"}
                                                        </span>
                                                    )}
                                                </Button>
                                            );
                                        })}
                                    </div>
                                </div>
                            );
                        })}
                    </>
                )}
            </div>
            
            <div className="flex items-center gap-2 pt-2 border-t">
                <Badge variant="success">
                    {userPermEntries.filter(e => e.state === "grant").length}
                </Badge>
                <span className="text-xs text-emerald-600">ممنوحة</span>
                <Badge variant="danger">
                    {userPermEntries.filter(e => e.state === "revoke").length}
                </Badge>
                <span className="text-xs text-red-500">مسحوبة</span>
            </div>
        </BaseFormDialog>
    );
}
