// Journal Entry types

export interface JournalEntry {
    journalEntryID: number;
    entryNumber: string;
    entryDate: string;
    description: string;
    reference?: string;
    totalDebit: number;
    totalCredit: number;
    status: JournalEntryStatus;
    statusName: string;
    entryTypeName?: string;
    createdByUserName?: string;
    createdAt: string;
    postedByUserName?: string;
    postedAt?: string;
    lines: JournalEntryLine[];
}

export interface JournalEntryLine {
    lineId: number;
    journalEntryID: number;
    accountCode: string;
    accountName: string;
    accountId: number;
    description?: string;
    debit: number;
    credit: number;
    costCenterCode?: string;
    costCenterName?: string;
}

export type JournalEntryStatus = 'Draft' | 'Posted' | 'Reversed';

export interface JournalEntryFilter {
    entryNumber?: string;
    status?: JournalEntryStatus;
    fromDate?: string;
    toDate?: string;
    accountId?: number;
    search?: string;
    pageNumber?: number;
    pageSize?: number;
}

export interface CreateJournalEntryDto {
    entryDate: string;
    description: string;
    reference?: string;
    entryTypeId?: number;
    lines: CreateJournalEntryLineDto[];
}

export interface CreateJournalEntryLineDto {
    accountId: number;
    accountCode: string;
    description?: string;
    debit: number;
    credit: number;
    costCenterId?: number;
}
