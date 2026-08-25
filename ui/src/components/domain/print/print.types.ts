
// ─── Shared Print Types ────────────────────────────────

export interface RetireeDto {
    retireeID: number;
    nationalID: string;
    fullName: string;
    militaryNumber: string | null;
    gender: number;
    phone: string | null;
    address: string | null;
    city: string | null;
    governorate: string | null;
    isAlive: boolean;
    deathDate: string | null;
    socialStatusName: string | null;
    healthStatusName: string | null;
    createdAt: string;
    unitName?: string | null;
    categoryName?: string | null;
    linkNumber?: string | null;
    rankName?: string | null;
}

export interface ApprovalItem {
    approvalItemID: number;
    batchNumber?: string | null;
    militaryNumber: string;
    fullName: string;
    rankName?: string | null;
    unitName?: string | null;
    retirementDate?: string | null;
    salary?: number | null;
    categoryName?: string | null;
    notes?: string | null;
    isProcessed: boolean;
    processedAt?: string | null;
    enrollmentDate?: string | null;
    linkNumber?: string | null;
}

export interface RetirementFileSummary {
    fileID: number;
    fileNumber: string;
    statusName?: string | null;
    fileStatusName?: string | null;
    rankName: string | null;
    branchName: string | null;
    fileOpenDate: string;
    activePaymentBookNumber?: string | null;
}

export interface SettlementSummary {
    settlementItemID: number;
    batchNumber: string | null;
    decisionNumber: string | null;
    decisionDate: string | null;
    decisionType: string | null;
    currentRankName: string | null;
    processedRankName: string | null;
    notes: string | null;
    createdAt: string;
}

export interface EntitlementSummary {
    entitlementID: number;
    rankName: string | null;
    amount: number;
    effectiveDate: string;
    endDate: string | null;
}

export interface EntitledForPrint {
    entitledId?: number;
    entitledID?: number;
    fullName: string;
    nationalID?: string;
    dateOfBirth?: string;
    relationshipName?: string;
    socialStatusName?: string | null;
    isEligible?: boolean;
    isActive?: boolean;
    personalPhotoPath?: string | null;
}

export interface AgentForPrint {
    agentId?: number;
    agentID?: number;
    fullName: string;
    nationalID?: string;
    phone?: string | null;
    agencyNumber?: string;
    agencyDate?: string;
    agencyEndDate?: string | null;
    notes?: string | null;
    personalPhotoPath?: string | null;
    isActive?: boolean;
}

export interface GuardianMinorForPrint {
    entitledId: number;
    entitledPersonName: string;
    isActive: boolean;
    startDate: string;
    endDate?: string | null;
}

export interface GuardianForPrint {
    guardianId?: number;
    guardianID?: number;
    fullName: string;
    nationalID?: string;
    phone?: string | null;
    guardianshipDocNumber?: string;
    startDate?: string;
    endDate?: string | null;
    personalPhotoPath?: string | null;
    isActive?: boolean;
    minors?: GuardianMinorForPrint[];
}

export interface SalaryRecipientForPrint {
    recipientId: number;
    recipientName: string;
    recipientType: string;
    isActive: boolean;
    agentId?: number;
    guardianId?: number;
    entitledId?: number;
}

export interface Statement {
    retiree: RetireeDto;
    approvalItems: ApprovalItem[];
    retirementFiles: RetirementFileSummary[];
    settlements: SettlementSummary[];
    entitlements: EntitlementSummary[];
}

export interface FileDetailsForPrint {
    file: {
        activePaymentBookNumber: string | null;
        personalPhotoPath: string | null;
    };
    entitled: EntitledForPrint[];
    agents: AgentForPrint[];
    guardians: GuardianForPrint[];
    recipients: SalaryRecipientForPrint[];
}

export interface NormalizableItem {
    agentId?: number;
    agentID?: number;
    AgentID?: number;
    guardianId?: number;
    guardianID?: number;
    GuardianID?: number;
    entitledId?: number;
    entitledID?: number;
    EntitledID?: number;
    recipientId?: number;
    recipientID?: number;
    RecipientID?: number;
    isActive?: boolean | number;
    IsActive?: boolean | number;
    fullName?: string;
    FullName?: string;
    personalPhotoPath?: string | null;
    PersonalPhotoPath?: string;
    personalPhoto?: string;
    PersonalPhoto?: string;
    PhotoPath?: string;
    photoPath?: string;
    path?: string;
    filePath?: string;
    FilePath?: string;
    recipientName?: string;
    RecipientName?: string;
    [key: string]: unknown;
}
