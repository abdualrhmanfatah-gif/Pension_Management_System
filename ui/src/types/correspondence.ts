import { z } from "zod";

import type { ApiResponse, PagedResult } from "@/types/api";

export type { ApiResponse, PagedResult };

// ─── Enums (using const object for erasable syntax) ──────

export const CorrespondenceDirection = {
  Outgoing: 0,
  Incoming: 1,
} as const;

export type CorrespondenceDirection = (typeof CorrespondenceDirection)[keyof typeof CorrespondenceDirection];

// ─── Interfaces ────────────────────────────────────────

export interface CorrespondenceTypeDto {
  correspondenceTypeID: number;
  code: string;
  name: string;
}

export interface ExternalPartyDto {
  externalPartyID: number;
  name: string;
  partyType: string;
  contactInfo?: string;
  isActive: boolean;
}

export interface CorrespondenceListItemDto {
  correspondenceID: number;
  direction: number;
  typeName: string;
  sequentialNumber: number;
  referenceNumber: string;
  correspondenceDate: string;
  subject: string;
  pageNumber?: string;
  externalPartyName?: string;
  employeeName?: string;
  createdByUserName: string;
  createdAt: string;
}

export interface CorrespondenceDetailsDto extends CorrespondenceListItemDto {
  correspondenceTypeID: number;
  externalPartyID?: number | null;
  employeeID?: number | null;
  notes?: string | null;
  documentCount: number;
}

export interface CreateCorrespondenceDto {
  direction: number;
  correspondenceTypeID: number;
  sequentialNumber: number;
  referenceNumber: string;
  correspondenceDate: string;
  subject: string;
  pageNumber?: string | null;
  externalPartyID?: number | null;
  employeeID?: number | null;
  notes?: string | null;
}

export type UpdateCorrespondenceDto = CreateCorrespondenceDto;

export interface CreateExternalPartyDto {
  name: string;
  partyType: string;
  contactInfo?: string | null;
}

export type UpdateExternalPartyDto = CreateExternalPartyDto;

// ─── Zod Schemas (Arabic Messages) ────────────────────

export const correspondenceSchema = z.object({
  direction: z.number({ message: "الاتجاه مطلوب" }),
  correspondenceTypeID: z.number().min(1, "نوع المراسلة مطلوب"),
  sequentialNumber: z.number().min(1, "الرقم المتسلسل يجب أن يكون أكبر من 0"),
  referenceNumber: z.string().min(1, "رقم القيد مطلوب").max(50, "رقم القيد طويل جداً"),
  correspondenceDate: z.string().min(1, "تاريخ المراسلة مطلوب"),
  subject: z.string().min(1, "الموضوع مطلوب").max(1000, "الموضوع طويل جداً"),
  pageNumber: z.string().max(50, "رقم الصفحة طويل جداً").optional().nullable(),
  externalPartyID: z.number().optional().nullable(),
  employeeID: z.number().optional().nullable(),
  notes: z.string().max(2000, "الملاحظات طويلة جداً").optional().nullable(),
});

export const externalPartySchema = z.object({
  name: z.string().min(1, "الاسم مطلوب").max(300, "الاسم طويل جداً"),
  partyType: z.string().min(1, "نوع الجهة مطلوب").max(100, "نوع الجهة طويل جداً"),
  contact_info: z.string().max(1000, "معلومات التواصل طويلة جداً").optional().nullable(),
});

// ─── API Response Wrappers ───────────────────────────

export type CorrespondenceListResponse = ApiResponse<PagedResult<CorrespondenceListItemDto>>;
export type CorrespondenceDetailsResponse = ApiResponse<CorrespondenceDetailsDto>;
export type ExternalPartyListResponse = ApiResponse<ExternalPartyDto[]>;
export type ExternalPartyResponse = ApiResponse<ExternalPartyDto>;
export type CorrespondenceTypeListResponse = ApiResponse<CorrespondenceTypeDto[]>;
