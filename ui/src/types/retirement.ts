import type { RetirementFileFormData } from "@/lib/validation/retirementFileSchema";

// Central domain types for the retirement module

export interface RetirementFileListItem {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    nationalID: string | null;
    militaryNumber: string | null;
    rankName: string | null;
    statusID: number | null;
    fileStatusName: string | null;
    fileOpenDate: string | null;
    missingDocumentsCount?: number;
    hasDocuments?: boolean;
    lastUploadedByUserName?: string | null;
    missingDataCount?: number;
    reviewStatus?: number;
}

export interface RetirementFilesPagedResult {
    items: RetirementFileListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
    nextCursor?: number | null;
    hasMore?: boolean;
}

export interface RetirementFileDetail {
    salaryStatus?: string;
    previousStatusID?: number | null;
    missingDataList?: string[];
    fileID: number;
    fileNumber: string;
    retireeID: number;
    retireeFullName: string;
    nationalID: string | null;
    rankName: string | null;
    militaryNumber: string | null;
    branchName: string | null;
    fileStatusName: string | null;
    retirementDate: string | null;
    serviceStartDate: string | null;
    serviceEndDate: string | null;
    fileOpenDate?: string | null;
    closedDate?: string | null;
    statusReason?: string | null;
    notes: string | null;
    createdAt: string;
    updatedAt?: string | null;
    assignedEmployeeID?: number | null;
    assignedEmployeeName?: string | null;
    activePaymentBookNumber?: string | null;
    activePaymentBookExpiry?: string | null;
    activePaymentBookStatus?: string | null;
    birthDate?: string | null;
    gender?: number;
    address?: string | null;
    deathDate?: string | null;
    isAlive?: boolean;
    entitledPersonsCount?: number;
    agentsCount?: number;
    guardiansCount?: number;
    missingDocumentsCount?: number;
    missingDataCount?: number;
    notesCount?: number;
    pledgesCount?: number;
    documentsCount?: number;
    suspensionsCount?: number;
    recipientsCount?: number;
    statusTransitionsCount?: number;
    settlementsCount?: number;
    paymentBooksCount?: number;
    salaryPaymentsCount?: number;
    entitlementsCount?: number;
    reviewCount?: number;
    achievementsCount?: number;
    reviewStatus?: number;
    reviewRequestedAt?: string | null;
    reviewCompletedAt?: string | null;
    reviewNotes?: string | null;
    reviewedByUser?: string | null;
    personalPhotoPath?: string | null;
    idDocumentPath?: string | null;
    rankID?: number | null;
    pensionBranchID?: number | null;
    statusID?: number | null;
    hasMinors?: boolean;
    hasPledgesOrExemptions?: boolean;
    hasSettlements?: boolean;
    statusName?: string | null;
    missingDocumentsList?: string[];
}

export interface RetireeDetail {
    retireeID: number;
    fullName: string;
    nationalID: string;
    militaryNumber: string | null;
    unitName?: string | null;
    categoryName?: string | null;
    rankName?: string | null;
    linkNumber?: string | null;
    gender: number;
    phone: string | null;
    address: string | null;
    city: string | null;
    governorate: string | null;
    isAlive: boolean;
    deathDate: string | null;
    socialStatusName: string | null;
    socialStatusID?: number;
    healthStatusName: string | null;
    healthStatusID?: number;
    personalPhotoPath?: string | null;
    pendingReviewStatus?: number;
}

export interface FileStatus {
    statusID: number;
    statusName: string;
    statusColor: string | null;
}

// Full entitled-person DTO returned by GET /entitled-persons/{id}
export interface EntitledPersonDto {
    entitledID: number;
    fileID: number;
    fullName: string;
    nationalID: string;
    relationshipID: number;
    relationshipName?: string;
    dateOfBirth?: string;
    ageYears?: number;
    phone?: string;
    socialStatusID?: number;
    socialStatusName?: string;
    healthStatusID?: number;
    healthStatusName?: string;
    isEligible: boolean;
    isDeceased: boolean;
    stopDate?: string;
    stopReason?: string;
    stopCode?: string;
    hasDisabilityException: boolean;
    hasActiveGuardian: boolean;
    hasNationalIdDocument: boolean;
    hasPersonalPhoto: boolean;
    personalPhotoPath?: string;
    nationalIdImageUrl?: string;
    pendingReviewStatus: number;
    updatedAt?: string;
}

// Lightweight related-entity summaries used in file details

export interface EntitledPersonSummary {
    entitledID: number;
    fileID?: number;
    fullName: string;
    nationalID?: string | null;
    relationshipName?: string;
    phone?: string | null;
    dateOfBirth?: string | null;
    ageYears?: number;
    socialStatusID?: number | null;
    socialStatusName?: string | null;
    healthStatusID?: number | null;
    healthStatusName?: string | null;
    personalPhotoPath?: string | null;
    isEligible?: boolean;
    isActive?: boolean;
    isDeleted?: boolean;
    hasNationalIdDocument?: boolean;
    hasPersonalPhoto?: boolean;
    isDeceased?: boolean;
    pendingReviewStatus?: number;
    sharePercent?: number | null;
}

export interface EntitledPersonSearchResult {
    entitledID: number;
    fileID: number;
    fullName: string;
    nationalID: string;
    phone?: string | null;
    relationshipName?: string | null;
    dateOfBirth?: string | null;
    isActive: boolean;
    isDeceased: boolean;
    personalPhotoPath?: string | null;
    fileNumber: string;
    retireeMilitaryNumber?: string | null;
}

export interface AgentSummary {
    agencyScope?: import("@/lib/agentScope").AgencyScope | null;
    entitledID?: number | null;
    entitledPersonName?: string | null;
    agentID: number;
    fullName: string;
    nationalID?: string;
    phone?: string | null;
    agencyNumber?: string | null;
    agencyDate?: string | null;
    personalPhotoPath?: string | null;
    isActive?: boolean;
    isDeleted?: boolean;
    pendingReviewStatus?: number;
}

export interface AgentDto {
    agencyScope?: import("@/lib/agentScope").AgencyScope | null;
    entitledID?: number | null;
    entitledPersonName?: string | null;
    agentID: number;
    fullName: string;
    nationalID: string;
    phone?: string | null;
    agencyNumber?: string | null;
    agencyDate?: string | null;
    agencyEndDate?: string | null;
    notes?: string | null;
    personalPhotoPath?: string | null;
    nationalIDDocumentPath?: string | null;
    agencyDocumentPath?: string | null;
    pendingReviewStatus?: number;
    updatedAt?: string;
}

export interface AgentSearchResult {
    agencyScope?: import("@/lib/agentScope").AgencyScope | null;
    entitledID?: number | null;
    entitledPersonName?: string | null;
    isActive: boolean;
    agentID: number;
    fullName: string;
    nationalID: string;
    phone?: string | null;
    agencyNumber?: string | null;
    agencyDate?: string | null;
    agencyEndDate?: string | null;
    notes?: string | null;
    personalPhotoPath?: string | null;
    nationalIDDocumentPath?: string | null;
    agencyDocumentPath?: string | null;
    fileID: number;
    fileNumber: string;
    retireeMilitaryNumber?: string | null;
}

export interface GuardianSummary {
    guardianID: number;
    fullName: string;
    nationalID?: string;
    phone?: string | null;
    guardianshipDocNumber?: string;
    personalPhotoPath?: string | null;
    isActive?: boolean;
    isDeleted?: boolean;
    pendingReviewStatus?: number;
}

export interface GuardianSearchResult {
    guardianID: number;
    fileID: number;
    fullName: string;
    nationalID: string;
    phone?: string | null;
    guardianshipDocNumber: string;
    isActive: boolean;
    personalPhotoPath?: string | null;
    fileNumber: string;
    retireeMilitaryNumber?: string | null;
}

export interface GuardianMinorDto {
    entitledID: number;
    entitledPersonName: string;
    isActive: boolean;
    startDate: string;
    endDate?: string | null;
}

export interface GuardianDto {
    guardianID: number;
    fileID: number;
    minors: GuardianMinorDto[];
    fullName: string;
    nationalID: string;
    personalPhotoPath?: string | null;
    phone?: string | null;
    guardianshipDocNumber: string;
    startDate: string;
    endDate?: string | null;
    isActive: boolean;
    pendingReviewStatus: number;
}

export type { RetirementFileFormData };

// Missing documents & pledges/exemptions/settlements (detailed models)

export interface MissingDocumentInfo {
    missingDocumentID: number;
    documentTypeID: number;
    documentTypeName: string;
    status: string; // Pending | Pledged | Exempted | Fulfilled
    pledgeID?: number | null;
    exemptionID?: number | null;
    notes?: string | null;
    createdAt: string;
    entityType?: string | null;
    entityName?: string | null;
    retireeID?: number | null;
    agentName?: string | null;
    agentID?: number | null;
    guardianName?: string | null;
    guardianID?: number | null;
    entitledName?: string | null;
    entitledID?: number | null;
    isAutoDetected?: boolean;
    isRequired?: boolean;
}

export interface MissingDocumentSummary extends MissingDocumentInfo {
    documentTypeID: number;
    isAutoDetected?: boolean;
    entityType?: string;
    entityName?: string;
}

export interface PledgeItem {
    pledgeID: number;
    fileID: number;
    retireeName?: string;
    missingDocuments?: MissingDocumentInfo[];
    pledgeDate: string;
    deadline: string;
    status: string;
    pledgedByName?: string;
    pledgedByType?: string;
    reason?: string;
    fulfilledAt?: string;
    pledgeNumber?: string;
}

export interface ExemptionItem {
    exemptionID: number;
    missingDocuments?: MissingDocumentInfo[];
    directiveSource: string;
    exemptionDate: string;
    notes?: string;
    createdAt: string;
}

export interface LookupDto {
    id: number;
    name: string;
}

export interface DocumentTypeLookup {
    id: number;
    name: string;
}

export interface DocumentItem {
    documentID: number;
    documentTypeID: number;
    documentTypeName: string;
    fileName: string;
    filePath?: string | null;
    notes?: string | null;
    uploadedByUser?: string | null;
    entityName?: string | null;
    entityType?: string | null;
    entitledID?: number | null;
    agentID?: number | null;
    guardianID?: number | null;
    retireeID?: number | null;
    documentNumber?: string | null;
    issueDate?: string | null;
    expiryDate?: string | null;
    createdAt: string;
    pendingReviewStatus?: number;
}

export interface StatusTransition {
    transitionID: number;
    oldStatusName: string | null;
    newStatusName: string | null;
    reason: string | null;
    changedByUser: string | null;
    changedAt: string;
}

export interface SuspensionItem {
    suspensionID: number;
    fileID: number;
    fileNumber?: string;
    suspensionDate: string;
    suspensionReason: string;
    suspensionSource: string;
    status: string;
    endDate?: string;
    endReason?: string;
    createdByUser?: string;
    endedByUser?: string;
    createdAt: string;
    linkedPledge?: {
        pledgeID: number;
        pledgeDate: string;
        deadline: string;
        status: string;
        pledgedByType: string;
        pledgedByName?: string;
        missingDocumentCount: number;
    };
}

export interface RecipientItem {
    recipientName?: string;
    recipientType?: string;
    effectiveEntitledID?: number | null;
    entitledPersonName?: string | null;
    recipientID: number;
    fileID: number;
    retireeID?: number;
    retireeFullName?: string;
    agentID?: number;
    agentFullName?: string;
    entitledID?: number;
    entitledFullName?: string;
    guardianID?: number;
    guardianFullName?: string;
    startDate: string;
    endDate?: string;
    isActive: boolean;
    notes?: string;
    pendingReviewStatus?: number;
    sharePercent?: number | null;
}

export interface NoteItem {
    noteID: number;
    fileID: number;
    content: string;
    createdByUser: string;
    createdAt: string;
}

export interface FinancialEntitlement {
    entitlementID: number;
    retireeID: number;
    retireeFullName?: string;
    amount: number;
    effectiveDate: string;
    endDate?: string | null;
    isActive: boolean;
    notes?: string | null;
    createdAt: string;
}

export interface SalaryPaymentBatch {
    batchID: number;
    batchMonth: string;
    status: string;
    totalAmount: number;
    recipientCount: number;
    createdAt: string;
    approvedAt?: string | null;
}

export interface FileReviewInfo {
    reviewStatus: number;
    reviewRequestedAt: string | null;
    reviewCompletedAt: string | null;
    reviewNotes: string | null;
    reviewedByUser: string | null;
}

export interface FieldChangeDetail {
    fieldName: string;
    oldValue: string | null;
    newValue: string | null;
    isMedia?: boolean;
}

export interface FieldChange {
    isNew: boolean;
    fieldName: string;
    oldValue: string | null;
    newValue: string | null;
    itemId?: number | null;
    itemStatus?: number | null;
    fieldChangesJson?: string | null;
    fieldDetails?: FieldChangeDetail[] | null;
    modifiedByUserName?: string | null;
}

export interface FileReviewDiffSection {
    sectionName: string;
    sectionType: string;
    entityDisplayName?: string | null;
    fieldChanges: FieldChange[];
}

export interface FileReviewDiff {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    reviewRequestedAt: string | null;
    reviewRequestedByUserName?: string | null;
    sections: FileReviewDiffSection[];
}

export interface PendingReviewFile {
    fileID: number;
    fileNumber: string;
    retireeFullName: string;
    nationalID: string | null;
    reviewStatus: number;
    reviewRequestedAt: string | null;
    reviewRequestedByUserName: string | null;
    reviewNotes: string | null;
}

export interface ChangeLogItem {
    id: number;
    changeType: string;
    description: string;
    entityName?: string | null;
    entityKey?: number | null;
    entityDisplayName?: string | null;
    oldStatusName?: string | null;
    newStatusName?: string | null;
    reason?: string | null;
    fieldChanges?: { fieldName: string; oldValue?: string | null; newValue?: string | null }[];
    changeDate: string;
    changedByUserName?: string | null;
    ipAddress?: string | null;
    userAgent?: string | null;
    correlationId?: string | null;
}

export interface PaymentBookItem {
    paymentBookID: number;
    fileID: number;
    bookNumber: string;
    issueDate: string;
    expiryDate: string;
    status: string;
    bookType: string;
    legacyColor?: string | null;
    changeReason?: string | null;
    recipientName?: string | null;
    recipientType?: string | null;
    attachmentPath?: string | null;
    notes?: string | null;
    issuingBranchID?: number | null;
    issuingBranchName?: string | null;
    createdAt: string;
    pendingReviewStatus?: number;
}

// ─── أنواع إعادة التصميم ───

export type ReviewSection = 'basic' | 'people' | 'documents' | 'financial' | 'history';

export type PersonType = 'entitled' | 'agent' | 'guardian';

export type PersonDocFilter = 'all' | 'uploaded' | 'missing';

export interface PersonSummary {
    id: number;
    type: PersonType;
    name: string;
    nationalID?: string | null;
    relationship?: string | null;
    age?: number | null;
    dateOfBirth?: string | null;
    socialStatus?: string | null;
    phone?: string | null;
    status: string;
    isActive: boolean;
    pendingReviewStatus?: number;
    photoPath?: string | null;
    createdAt?: string;
    agencyNumber?: string | null;
    agencyDate?: string | null;
    guardianshipDocNumber?: string | null;
    isEligible?: boolean;
    isDeceased?: boolean;
}

export interface ReviewItemStatus {
    PENDING: 0;
    APPROVED: 1;
    REJECTED: 2;
}

export interface ReviewItemStatusMap {
    [key: string]: number;
}

export const REVIEW_ITEM_STATUS: ReviewItemStatus = {
    PENDING: 0,
    APPROVED: 1,
    REJECTED: 2,
};

export interface FileReviewItem {
    itemId: number;
    retirementFileID: number;
    entityType?: number | null;
    entityID?: number | null;
    fieldName: string;
    oldValue: string | null;
    newValue: string | null;
    sectionName?: string | null;
    sectionType?: string | null;
    entityDisplayName?: string | null;
    fieldChangesJson?: string | null;
    status: number;
    reviewedByUserID?: number | null;
    reviewedAt?: string | null;
    reviewNotes?: string | null;
}

export interface ReviewSectionData {
    sectionName: string;
    sectionType: string;
    items: FileReviewItem[];
    approvedCount: number;
    rejectedCount: number;
    pendingCount: number;
}

export interface PersonDocumentsResult {
    uploaded: DocumentItem[];
    missing: MissingDocumentInfo[];
    pledges: PledgeItem[];
}
