// Fiscal Period types

export interface FiscalPeriod {
    periodId: number;
    periodName: string;
    periodNumber: number;
    fiscalYearId: number;
    fiscalYearName: string;
    startDate: string;
    endDate: string;
    isClosed: boolean;
    closedByUserName?: string;
    closedAt?: string;
    quarter: number;
}

export interface FiscalYear {
    fiscalYearId: number;
    yearName: string;
    startDate: string;
    endDate: string;
    isClosed: boolean;
    periods?: FiscalPeriod[];
}

export interface FiscalPeriodFilter {
    fiscalYearId?: number;
    isClosed?: boolean;
    search?: string;
}
