import api from "./axios";
import type { PagedResult } from "@/types/api";

export interface UserDto {
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
    assignments: any[];
}

export interface RoleOption {
    roleID: number;
    roleName: string;
    description: string | null;
}

export interface PermissionItem {
    permissionId: number;
    code: string;
    description: string | null;
    category: string | null;
}

export const usersApi = {
    getAll: (params: { page: number; pageSize: number }) =>
        api.get<{ data: PagedResult<UserDto> }>("/users", { params }).then(r => r.data.data),

    getRoles: () =>
        api.get<{ data: RoleOption[] }>("/roles").then(r => r.data.data || []),

    getPermissions: () =>
        api.get<{ data: PermissionItem[] }>("/permissions").then(r => r.data.data),

    getUserPermissions: (userId: number) =>
        api.get<{ data: { permissionId: number; isGranted: boolean }[] }>(`/users/${userId}/permissions`).then(r => r.data.data),

    updateUserPermissions: (userId: number, permissions: { permissionId: number; isGranted: boolean }[]) =>
        api.put(`/users/${userId}/permissions`, { permissions }),

    getRolePermissions: (roleId: number) =>
        api.get<{ data: { granularPermissions?: { permissionId: number }[] } }>(`/roles/${roleId}`).then(r => r.data.data),

    updateRolePermissions: (roleId: number, permissionIds: number[]) =>
        api.put(`/roles/${roleId}/permissions`, { permissionIds }),

    create: (data: any) =>
        api.post("/users", data),

    update: (id: number, data: any) =>
        api.put(`/users/${id}`, data),

    delete: (id: number) =>
        api.delete(`/users/${id}`),

    resetPassword: (userId: number, newPassword: string) =>
        api.post(`/users/${userId}/reset-password`, { newPassword }),
};
