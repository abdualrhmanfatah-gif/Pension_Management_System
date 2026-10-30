// Retirement File types

export interface RetirementFile {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    retireeId?: number;
    nationalID: string | null;
    militaryNumber?: string | null;
    rankName?: string | null;
    statusID: number;
    statusName: string;
    statusColor?: string;
    isAlive: boolean;
    deathDate?: string | null;
    createdAt: string;
    updatedAt?: string;
    branchId?: number;
    branchName?: string;
    assignedToUserName?: string;
    notes?: string;
}

export interface RetirementFileDetail extends RetirementFile {
    birthDate?: string;
    healthStatusID?: number;
    healthStatusName?: string;
    phone?: string;
    address?: string;
    personalPhotoPath?: string;
    entitledPersons?: EntitledPerson[];
    agents?: Agent[];
    guardians?: Guardian[];
    documents?: Document[];
    financialEntitlements?: FinancialEntitlement[];
}

export interface EntitledPerson {
    entitledID: number;
    fullName: string;
    nationalID?: string;
    relationship?: string;
    entitlementPercentage?: number;
    isActive: boolean;
    fileId: number;
}

export interface Agent {
    agentID: number;
    fullName: string;
    nationalID?: string;
    phone?: string;
    agencyNumber?: string;
    fileId: number;
}

export interface Guardian {
    guardianID: number;
    fullName: string;
    nationalID?: string;
    phone?: string;
    fileId: number;
    entitledPersons?: EntitledPerson[];
}

export interface Document {
    documentID: number;
    documentTypeName: string;
    filePath?: string;
    fileSize?: number;
    uploadedAt: string;
    uploadedByUserName?: string;
    isVerified?: boolean;
}

export interface FinancialEntitlement {
    entitlementId: number;
    entitlementType: string;
    amount: number;
    startDate?: string;
    endDate?: string;
    isActive: boolean;
}

export interface RetirementFileFilter {
    search?: string;
    statusId?: number;
    branchId?: number;
    isAlive?: boolean;
    pageNumber?: number;
    pageSize?: number;
}

export interface FileStatus {
    statusID: number;
    statusName: string;
    statusColor?: string;
}

// Review types
export interface FileReviewItem {
    itemId: number;
    fileId: number;
    sectionName?: string;
    fieldName?: string;
    entityDisplayName?: string;
    oldValue?: string;
    newValue?: string;
    status: ReviewItemStatus;
    reviewNotes?: string;
    fieldChangesJson?: string;
    reviewedAt?: string;
    reviewedByUserName?: string;
}

export interface FieldChangeDetail {
    fieldName: string;
    oldValue?: string;
    newValue?: string;
    isMedia?: boolean;
}

export type ReviewItemStatus = 'Pending' | 'Approved' | 'Rejected';

export const REVIEW_ITEM_STATUS = {
    PENDING: 'Pending' as const,
    APPROVED: 'Approved' as const,
    REJECTED: 'Rejected' as const,
};
