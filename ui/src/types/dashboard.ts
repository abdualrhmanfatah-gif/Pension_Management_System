export interface DashboardStats {
    totalRetirees: number;
    activeFilesCount: number;
    closedFilesCount: number;
    rejectedFilesCount: number;
    totalEntitledPersons: number;
    totalMonthlyPayments: number;
    activeUsersCount: number;
    activeBranchesCount: number;
    draftBatchesCount: number;
    approvedBatchesCount: number;
    totalPaidThisYear: number;
    activeEntitlementsCount: number;
    pendingSettlementsCount: number;
    approvedSettlementsCount: number;
    totalBudgetAmount: number;
    usedBudgetAmount: number;
}

export interface RecentActivity {
    logID: number;
    userName: string | null;
    action: string;
    entityName: string | null;
    entityDisplayName: string | null;
    createdAt: string;
}
