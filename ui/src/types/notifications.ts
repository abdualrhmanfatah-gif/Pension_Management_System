import type { PagedResult } from "@/types/api";

export type { PagedResult };

export interface NotificationAttachmentDto {
  attachmentID: number;
  notificationID: number;
  fileName: string;
  fileUrl: string;
  mimeType: string;
  fileSize: number;
  uploadedAt: string;
}

export interface NotificationDto {
  notificationID: number;
  userID: number;
  title: string;
  message: string;
  type?: string;
  category?: string;
  priority: number;
  actionUrl?: string;
  actionType?: string;
  payload?: string;
  isRead: boolean;
  createdAt: string;
  actorID?: number;
  actorName?: string;
  relatedRecordID?: number;
  attachments?: NotificationAttachmentDto[];
}

export const CATEGORY_LABELS: Record<string, string> = {
  WORKFLOW: "ملفات التقاعد",
  FINANCIAL: "مالية",
  SECURITY: "أمان",
  SYSTEM: "النظام",
  HR: "موارد بشرية",
  PENSIONS: "المعاشات",
};

export const CATEGORY_COLORS: Record<string, "info" | "success" | "danger" | "warning" | "ghost" | "secondary"> = {
  WORKFLOW: "info",
  FINANCIAL: "success",
  SECURITY: "danger",
  SYSTEM: "secondary",
  HR: "warning",
  PENSIONS: "ghost",
};
