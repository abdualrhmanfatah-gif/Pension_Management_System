import { useQuery } from "@tanstack/react-query";
import { Check,Search } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import { usersApi } from "@/api/users";
import { Badge, BaseFormDialog, Button, Input, Text } from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";

import { categoryColors,categoryLabels, getPermissionName } from "./permission-utils";

// ─── Types ─────────────────────────────────────────────

interface PermissionItem {
    permissionId: number;
    code: string;
    description: string | null;
    category: string | null;
}

interface RolePermissionsDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    roleId: number | null;
    roleName: string;
    onSaved: () => void;
}

export function RolePermissionsDialog({
    open,
    onOpenChange,
    roleId,
    roleName,
    onSaved
}: RolePermissionsDialogProps) {
    const [selectedPermIds, setSelectedPermIds] = useState<number[]>([]);
    const [searchTerm, setSearchTerm] = useState("");

    const { data: allPermissions = [], isLoading: loadingPermissions } = useQuery({
        queryKey: queryKeys.admin.permissions,
        queryFn: () => usersApi.getPermissions(),
        enabled: open
    });

    const { data: rolePermIds = [], isLoading: loadingUserPermissions } = useQuery<number[]>({
        queryKey: queryKeys.admin.rolePermissions(roleId!),
        queryFn: async () => {
            if (!roleId) return [];
            const role = await usersApi.getRolePermissions(roleId);
            const granularPerms = role?.granularPermissions || [];
            return granularPerms.map(p => p.permissionId);
        },
        enabled: open && !!roleId && allPermissions.length > 0,
    });

    useEffect(() => {
        if (rolePermIds.length > 0 || (open && roleId && !loadingUserPermissions)) {
            setSelectedPermIds(rolePermIds);
        }
    }, [rolePermIds, open, roleId, loadingUserPermissions]);

    const loading = loadingPermissions || loadingUserPermissions;

    const saveMutation = useApiMutation(
        (permissionIds: number[]) => usersApi.updateRolePermissions(roleId!, permissionIds),
        {
            successMessage: `تم تحديث صلاحيات "${roleName}"`,
            onSuccess: () => {
                onOpenChange(false);
                onSaved();
            }
        }
    );

    const togglePermission = (pid: number) => {
        setSelectedPermIds(prev =>
            prev.includes(pid) ? prev.filter(id => id !== pid) : [...prev, pid]
        );
    };

    const handleSave = () => {
        if (!roleId) return;
        saveMutation.mutate(selectedPermIds);
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
            title={`صلاحيات دور "${roleName}"`}
            description="حدد أو ألغِ الصلاحيات لهذا الدور — التغييرات ستؤثر على جميع المستخدمين ذوي هذا الدور"
            size="4xl"
            isLoading={saveMutation.isPending}
            onSubmit={handleSave}
            submitLabel="حفظ الصلاحيات"
        >
            <div className="space-y-4 h-[60dvh] overflow-y-auto pe-2">
                {loading ? (
                    <div className="flex justify-center py-10 text-muted-foreground italic">جاري تحميل الصلاحيات...</div>
                ) : (
                    <>
                        <div className="sticky top-0 z-10 bg-background pb-2">
                            <div className="relative">
                                <Search className="absolute start-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                                <Input 
                                    placeholder="بحث عن صلاحية بالكود أو الوصف..." 
                                    className="ps-9"
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                />
                            </div>
                        </div>

                        {Object.entries(groupedPermissions).map(([category, perms]) => (
                            <div key={category} className="space-y-2 border-b pb-4 last:border-0">
                                <div className="flex items-center justify-between gap-2 bg-muted/20 p-2 rounded-md">
                                    <div className="flex items-center gap-2">
                                        <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-semibold ${categoryColors[category] || "bg-muted text-muted-foreground"}`}>
                                            {categoryLabels[category] || category}
                                        </span>
                                        <span className="text-xs text-muted-foreground">
                                            ({perms.filter(p => selectedPermIds.includes(p.permissionId)).length}/{perms.length})
                                        </span>
                                    </div>
                                    <Button 
                                        variant="ghost" 
                                        size="sm" 
                                        className="h-7 text-[10px] sm:text-xs"
                                        onClick={() => {
                                            const categoryIds = perms.map(p => p.permissionId);
                                            const allSelected = categoryIds.every(id => selectedPermIds.includes(id));
                                            if (allSelected) {
                                                setSelectedPermIds(prev => prev.filter(id => !categoryIds.includes(id)));
                                            } else {
                                                setSelectedPermIds(prev => [...new Set([...prev, ...categoryIds])]);
                                            }
                                        }}
                                    >
                                        {perms.every(id => selectedPermIds.includes(id.permissionId)) ? "إلغاء الكل" : "تحديد الكل"}
                                    </Button>
                                </div>
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
                                    {perms.map(p => {
                                        const isSelected = selectedPermIds.includes(p.permissionId);
                                        return (
                                            <Button
                                                key={p.permissionId}
                                                type="button"
                                                variant={isSelected ? "default" : "outline"}
                                                className="justify-start gap-3 h-auto p-3"
                                                onClick={() => togglePermission(p.permissionId)}
                                            >
                                                <div className={`flex items-center justify-center w-5 h-5 rounded-md border-2 transition-all ${isSelected
                                                    ? "bg-primary border-primary text-white"
                                                    : "border-border dark:border-gray-600"
                                                    }`}>
                                                    {isSelected && <Check className="w-3 h-3" />}
                                                </div>
                                                <div className="flex-1 min-w-0 text-start">
                                                    <Text variant="label" className="leading-tight">{getPermissionName(p.code)}</Text>
                                                    <p className="text-[10px] sm:text-xs text-muted-foreground/70 font-mono mt-0.5">{p.code}</p>
                                                    {p.description && (
                                                        <p className="text-[11px] text-muted-foreground truncate mt-1">{p.description}</p>
                                                    )}
                                                </div>
                                            </Button>
                                        );
                                    })}
                                </div>
                            </div>
                        ))}
                    </>
                )}
            </div>
            
            <div className="flex items-center gap-2 pt-2">
                <Badge variant="info">{selectedPermIds.length}</Badge>
                <span className="text-xs text-muted-foreground">صلاحية محددة</span>
            </div>
        </BaseFormDialog>
    );
}
