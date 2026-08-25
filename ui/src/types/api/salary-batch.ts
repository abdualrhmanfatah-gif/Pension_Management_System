// Salary Batch types

export interface SalaryBatch {
    batchId: number;
    batchNumber: string;
    month: number;
    year: number;
    monthName?: string;
    status: SalaryBatchStatus;
    statusName: string;
    totalAmount: number;
    recipientCount: number;
    createdByUserName?: string;
    createdAt: string;
    approvedByUserName?: string;
    approvedAt?: string;
}

export interface SalaryRecipient {
    recipientId: number;
    retireeId?: number;
    retireeName: string;
    nationalId?: string;
    fileNumber?: string;
    fileId?: number;
    bankName?: string;
    bankAccountNumber?: string;
    amount: number;
    recipientType: string;
    isActive: boolean;
}

export interface SalaryBatchFilter {
    month?: number;
    year?: number;
    status?: SalaryBatchStatus;
    search?: string;
    pageNumber?: number;
    pageSize?: number;
}

export type SalaryBatchStatus = 'Draft' | 'PendingApproval' | 'Approved' | 'Paid' | 'Cancelled';

export const SALARY_BATCH_STATUS = {
    DRAFT: 'Draft' as const,
    PENDING_APPROVAL: 'PendingApproval' as const,
    APPROVED: 'Approved' as const,
    PAID: 'Paid' as const,
    CANCELLED: 'Cancelled' as const,
};

export interface CreateSalaryBatchDto {
    month: number;
    year: number;
    notes?: string;
}

export interface SalaryBatchSummary {
    totalBatches: number;
    totalAmount: number;
    totalRecipients: number;
    pendingApproval: number;
}
