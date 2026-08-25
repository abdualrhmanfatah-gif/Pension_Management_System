using System;
using System.Collections.Generic;

namespace PensionMS.Core.DTOs.Accounting;

public enum JournalEntryStatus
{
    Draft,
    Posted,
    Reversed
}

// ═══════════════════════════════════════
// CHART OF ACCOUNTS DTOs
// ═══════════════════════════════════════

public class ChartOfAccountDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public int AccountLevel { get; set; }
    public int? ParentAccountID { get; set; }
    public string? ParentAccountName { get; set; }
    public bool IsPostable { get; set; }
    public bool IsActive { get; set; }
    public string AccountCategory { get; set; } = "Asset";
    public string? Description { get; set; }
    public int SubAccountsCount { get; set; }
    public bool HasChildren => SubAccountsCount > 0;
    public DateTime CreatedAt { get; set; }
    public List<ChartOfAccountDto>? Children { get; set; }
}

public class CreateChartOfAccountDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public int? ParentAccountID { get; set; }
    public bool IsPostable { get; set; }
    public string AccountCategory { get; set; } = "Asset";
    public string? Description { get; set; }
}

public class UpdateChartOfAccountDto : CreateChartOfAccountDto
{
    public bool IsActive { get; set; }
}

// ═══════════════════════════════════════
// COST CENTER DTOs
// ═══════════════════════════════════════

public class CostCenterDto
{
    public int CostCenterID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentCostCenterID { get; set; }
    public string? ParentCostCenterName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCostCenterDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ParentCostCenterID { get; set; }
}

public class UpdateCostCenterDto : CreateCostCenterDto
{
    public bool IsActive { get; set; }
}

// ═══════════════════════════════════════
// FISCAL YEAR & PERIOD DTOs
// ═══════════════════════════════════════

public class FiscalYearSummaryDto
{
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalBudget { get; set; }
    public bool IsClosed { get; set; }
}

public class CreateFiscalYearDto
{
    public string YearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalBudget { get; set; }
}

public class FiscalPeriodDto
{
    public int PeriodID { get; set; }
    public int FiscalYearID { get; set; }
    public string? FiscalYearName { get; set; }
    public int? QuarterID { get; set; }
    public string? QuarterName { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public string? ClosedByUserName { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFiscalPeriodDto
{
    public int FiscalYearID { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

// ═══════════════════════════════════════
// QUARTER DTOs
// ═══════════════════════════════════════

public class QuarterDto
{
    public int QuarterID { get; set; }
    public int FiscalYearID { get; set; }
    public int QuarterNumber { get; set; }
    public string QuarterName { get; set; } = string.Empty;  // Q1 2026
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public List<FiscalPeriodDto> Periods { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetResult { get; set; }
}

public class CloseQuarterRequestDto
{
    public int QuarterID { get; set; }
}

// ═══════════════════════════════════════
// QUARTER DETAILS DTOs
// ═══════════════════════════════════════

public class QuarterDetailsDto
{
    public int QuarterID { get; set; }
    public int FiscalYearID { get; set; }
    public int QuarterNumber { get; set; }
    public string QuarterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }

    // ملخص الميزانية التشغيلية
    public decimal OpeningBalance { get; set; }        // الرصيد الافتتاحي
    public decimal TotalWithdrawn { get; set; }      // إجمالي السحب من البنك
    public decimal TotalSpent { get; set; }           // إجمالي الصرف الفعلي
    public decimal RemainingBalance { get; set; }     // المتبقي

    // تفصيل حسب الفترة
    public List<PeriodBreakdownDto> Periods { get; set; } = new();

    // تفصيل المصروفات حسب الحساب
    public List<ExpenseBreakdownDto> ExpenseBreakdown { get; set; } = new();
}

public class PeriodBreakdownDto
{
    public int PeriodID { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public decimal OpeningBalance { get; set; }       // الرصيد الافتتاحي
    public decimal Withdrawn { get; set; }            // سحب من البنك
    public decimal Spent { get; set; }                // صرف فعلي
    public decimal ClosingBalance { get; set; }       // الرصيد الختامي
}

public class ExpenseBreakdownDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }               // إجمالي المصروف في هذا الحساب
    public int EntryCount { get; set; }               // عدد القيود
}

// ═══════════════════════════════════════
// JOURNAL ENTRY TYPE DTOs
// ═══════════════════════════════════════

public class JournalEntryTypeDto
{
    public int JournalEntryTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string TypeNameEn { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateJournalEntryTypeDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string TypeName { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    public string TypeNameEn { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}

public class UpdateJournalEntryTypeDto : CreateJournalEntryTypeDto { }

// ═══════════════════════════════════════
// JOURNAL ENTRY DTOs
// ═══════════════════════════════════════

public class JournalEntryDto
{
    public int JournalEntryID { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public int PeriodID { get; set; }
    public string? PeriodName { get; set; }
    public int JournalEntryTypeID { get; set; }
    public string? JournalEntryTypeName { get; set; }
    public string? JournalEntryTypeColor { get; set; }
    public int? SourceReferenceID { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PostedByUserName { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? ReversedEntryID { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class JournalEntryLineDto
{
    public int LineID { get; set; }
    public int JournalEntryID { get; set; }
    public int LineNumber { get; set; }
    public int AccountID { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public int? CostCenterID { get; set; }
    public string? CostCenterName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public string? CheckNo { get; set; }
    public DateTime? CheckDueDate { get; set; }
    
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciledByUserName { get; set; }
}

public class ReverseJournalEntryRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class JournalEntryFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? JournalEntryTypeID { get; set; }
    public string? Status { get; set; }
    public int? PeriodID { get; set; }
    public int? AccountID { get; set; }
    public int? CreatedByUserID { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AccountBalanceDto
{
    public int BalanceID { get; set; }
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public int PeriodID { get; set; }
    public string? PeriodName { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public decimal NetBalance { get; set; }
}

public class CreateJournalEntryDto
{
    public DateTime EntryDate { get; set; }
    public int PeriodID { get; set; }
    public int JournalEntryTypeID { get; set; }
    public int? SourceReferenceID { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

public class CreateJournalEntryLineDto
{
    public int AccountID { get; set; }
    public int? CostCenterID { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public string? CheckNo { get; set; }
    public DateTime? CheckDueDate { get; set; }
    
}

// ═══════════════════════════════════════
// REPORT DTOs
// ═══════════════════════════════════════

public class TrialBalanceItemDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public int AccountLevel { get; set; }
    public int? ParentAccountID { get; set; }
    public bool IsPostable { get; set; }

    // الأرصدة الافتتاحية
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }

    // حركة الفترة
    public decimal MovementDebit { get; set; }
    public decimal MovementCredit { get; set; }

    // الأرصدة الختامية
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }

    // المجاميع الإجمالية (للت compatibility مع الأماكن التي تستخدمها)
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; }
    public string BalanceType { get; set; } = string.Empty; // "Debit" أو "Credit"
}



// ── Balance Sheet (الميزانية العمومية) ──
public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public List<BalanceSheetSectionDto> Assets { get; set; } = new();     // 1xxx accounts
    public List<BalanceSheetSectionDto> Liabilities { get; set; } = new(); // 2xxx accounts
    public List<BalanceSheetSectionDto> Equity { get; set; } = new();      // 3xxx accounts
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public bool IsBalanced { get; set; }  // Assets == Liabilities + Equity
}

public class BalanceSheetSectionDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int AccountLevel { get; set; }
    public bool IsPostable { get; set; }
    public decimal Balance { get; set; }
    public List<BalanceSheetSectionDto> Children { get; set; } = new();
}

// ── Income Statement (قائمة الدخل) ──
public class IncomeStatementDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<IncomeStatementLineDto> Revenues { get; set; } = new();  // 4xxx accounts
    public List<IncomeStatementLineDto> Expenses { get; set; } = new();  // 3xxx accounts
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }  // TotalRevenue - TotalExpenses
}

public class IncomeStatementLineDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int AccountLevel { get; set; }
    public decimal Amount { get; set; }
    public List<IncomeStatementLineDto> Children { get; set; } = new();
}

// ── General Ledger (دفتر الأستاذ العام) ──
public class GeneralLedgerDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<GeneralLedgerLineDto> Entries { get; set; } = new();
}

public class GeneralLedgerLineDto
{
    public DateTime EntryDate { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CostCenterName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
}

// ── Subledger Report (تقرير الأستاذ المساعد) ──
public class SubledgerReportLineDto
{
    public DateTime EntryDate { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class SubledgerReportResultDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<SubledgerReportLineDto> Entries { get; set; } = new();
}

// ── Account Statement (كشف حساب) ──
public class AccountStatementResultDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<AccountStatementItemDto> Items { get; set; } = new();
}

public class AccountStatementItemDto
{
    public DateTime EntryDate { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

// ── Journal Register (يومية القيود) ──
public class JournalRegisterDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalEntries { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalPages => (int)Math.Ceiling((double)TotalEntries / PageSize);
    public decimal GrandTotalDebit { get; set; }
    public decimal GrandTotalCredit { get; set; }
    public List<JournalRegisterItemDto> Entries { get; set; } = new();
}

public class JournalRegisterFilterDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class JournalRegisterItemDto
{
    public int JournalEntryID { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public int JournalEntryTypeID { get; set; }
    public string? JournalEntryTypeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

// ── Dashboard (لوحة التحكم) ──
public class AccountingDashboardDto
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public int TotalJournalEntries { get; set; }
    public int DraftEntries { get; set; }
    public int PostedEntries { get; set; }
    public int ReversedEntries { get; set; }
    public int OpenPeriods { get; set; }
    public int ClosedPeriods { get; set; }
    public List<MonthlyFinancialSummaryDto> MonthlySummary { get; set; } = new();
    public List<RecentJournalEntryDto> RecentEntries { get; set; } = new();
    public List<CostCenterSummaryDto> CostCenterBreakdown { get; set; } = new();
}

public class MonthlyFinancialSummaryDto
{
    public string Month { get; set; } = string.Empty;  // "يناير 2026"
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome { get; set; }
}

public class RecentJournalEntryDto
{
    public int JournalEntryID { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public int JournalEntryTypeID { get; set; }
    public string? JournalEntryTypeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
}

public class CostCenterSummaryDto
{
    public string CostCenterName { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

// ═══════════════════════════════════════
// CLOSING PROCEDURES DTOs
// ═══════════════════════════════════════

public class YearEndCloseDto
{
    public int FiscalYearID { get; set; }
    public int TargetAccountID { get; set; }  // The surplus/deficit account (3400)
}

public class OpeningBalanceDto
{
    public int NewFiscalYearID { get; set; }
    public int SourceFiscalYearID { get; set; }
}

public class ClosingResultDto
{
    public int JournalEntryID { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public decimal TotalRevenuesClosed { get; set; }
    public decimal TotalExpensesClosed { get; set; }
    public decimal NetResult { get; set; }  // Surplus or Deficit
    public string ResultType { get; set; } = string.Empty;  // "فائض" or "عجز"
}

public class OpeningBalanceResultDto
{
    public int JournalEntryID { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public int AccountsCarriedForward { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
}

/// <summary>
/// نتيجة إدراج البيانات الأولية الافتراضية (الأصول ورصيد البنك المركزي) للسنة المالية
/// </summary>
public class SeedInitialDefaultsResultDto
{
    public int FiscalYearID { get; set; }
    public int PeriodID { get; set; }
    public int BalanceCount { get; set; }
    public int AssetCount { get; set; }
    public decimal CentralBankBalance { get; set; }
    public List<SeedInitialAssetDto> Assets { get; set; } = new();
    public List<SeedInitialBalanceLineDto> Balances { get; set; } = new();
}

public class SeedInitialAssetDto
{
    public string AssetNumber { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal AcquiredCost { get; set; }
}

public class SeedInitialBalanceLineDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class SeedInitialDefaultsRequestDto
{
    public int FiscalYearID { get; set; }
}

// ═══════════════════════════════════════
// FINANCIAL OPERATIONS DTOs
// ═══════════════════════════════════════

/// <summary>
/// أنواع العمليات المالية المدعومة في النظام
/// </summary>
public enum FinancialOperationType
{
    // أولاً: عمليات المعاشات والمزايا
    SalaryPermanent = 1,        // معاشات دائمين
    SalaryContract = 2,         // معاشات متعاقدين
    SalaryTemporary = 3,        // معاشات مؤقتين
    AllowanceRepresentation = 4,// بدل تمثيل
    AllowanceHousing = 5,       // بدل سكن
    AllowanceNature = 6,        // بدل طبيعة عمل
    AllowanceExtra = 7,         // بدل إضافي
    AllowanceOther = 8,         // بدلات أخرى
    StaffBonus = 9,             // مكافآت موظفين
    StaffBenefits = 10,         // مزايا عينية
    ExpertFees = 11,            // أتعاب خبراء
    SocialInsurance = 12,       // تأمينات اجتماعية

    // ثانياً: عمليات صرف التقاعد
    PensionDisbursement = 20,   // صرف معاشات تقاعدية
    EndOfServiceBonus = 21,     // مكافأة نهاية خدمة
    InjuryCompensation = 22,    // تعويض إصابة/وفاة

    // ثالثاً: عمليات المصروفات السلعية
    ExpenseElectricity = 30,    // كهرباء
    ExpenseWater = 31,          // مياه
    ExpenseFuel = 32,           // وقود
    ExpenseSpareParts = 33,     // قطع غيار
    ExpenseStationery = 34,     // قرطاسية
    ExpenseRawMaterials = 35,   // مواد خام

    // رابعاً: عمليات المصروفات الخدمية
    ExpenseMaintenance = 40,    // صيانة
    ExpenseAdvertising = 41,    // إعلان
    ExpenseTravel = 42,         // نقل وسفر
    ExpenseRent = 43,           // إيجار
    ExpenseGov = 44,            // رسوم حكومية
    ExpenseMisc = 45,           // خدمات متنوعة
    ExpenseResearch = 46,       // أبحاث ودراسات
    ExpenseOutsource = 47,      // تشغيل خارجي

    // خامساً: عمليات تحصيل الإيرادات
    RevenuePensionSubscription = 50, // اشتراكات تقاعدية
    RevenueBankInterest = 51,        // فوائد بنكية
    RevenueInvestment = 52,          // أرباح استثمار
    RevenueLocalSecurities = 53,     // أوراق مالية محلية
    RevenueForeignSecurities = 54,   // أوراق مالية خارجية
    RevenuePropertyRent = 55,        // إيجار عقار
    RevenueCurrencyDiff = 56,        // فروق عملة
    RevenueCommissions = 57,         // عمولات
    RevenueSalesScraps = 58,         // بيع مخلفات
    RevenueBadDebt = 59,             // استرداد ديون معدومة
    RevenueGrantLocal = 60,          // إعانة محلية
    RevenueGrantForeign = 61,        // إعانة خارجية
    RevenueInsuranceClaim = 62,      // تعويض تأمين
    RevenueCapitalGain = 63,         // إيراد رأسمالي

    // سادساً: عمليات التأمينات والذمم
    SecurityReceiveTender = 70,      // استلام تأمين مناقصة
    SecurityReturnTender = 71,       // رد تأمين مناقصة
    SecurityReceiveContractor = 72,  // استلام تأمين مقاول
    SecurityReturnContractor = 73,   // رد تأمين مقاول
    EmployeeCustodiesDeduct = 74,    // استقطاع أمانات عاملين
    EmployeeCustodiesSettle = 75,    // تسديد أمانات عاملين
    ContractorPenalty = 76,          // توقيف غرامة مقاول
    PayablePurchaseLocal = 77,       // شراء بالآجل محلي
    PayableSettleLocal = 78,         // سداد ذمة مورد محلي
    PayablePurchaseForeign = 79,       // شراء بالآجل خارجي
    PayableSettleForeign = 80,       // سداد ذمة مورد خارجي

    // سابعاً: التسويات
    AccrualSalaries = 90,            // إثبات معاشات مستحقة
    AccrualSalariesSettle = 91,      // سداد معاشات مستحقة
    AccrualMaintenance = 92,         // فاتورة صيانة مستحقة
    DeferredRentReceive = 93,        // تحصيل إيجار مقدماً
    DeferredRentEarn = 94,           // تحقق إيراد إيجار مقدم
    DeferredGrantReceive = 95,       // استلام إعانة مقدماً
    DeferredGrantEarn = 96,          // تحقق إعانة مقدمة
    ProvisionRecovery = 97,          // استرداد مخصصات

    // ثامناً: الإقفال والتوزيع
    LegalReserve = 100,             // احتياطي قانوني
    GeneralReserve = 101,           // احتياطي عام
    GovShare = 102,                 // حصة الحكومة
    ProfitCarryForward = 103,       // أرباح مرحلة
}

/// <summary>
/// طلب تنفيذ عملية مالية موحدة
/// </summary>
public class FinancialOperationRequestDto
{
    /// <summary>نوع العملية</summary>
    public FinancialOperationType OperationType { get; set; }

    /// <summary>المبلغ</summary>
    public decimal Amount { get; set; }

    /// <summary>معرف الفترة المحاسبية</summary>
    public int PeriodID { get; set; }

    /// <summary>تاريخ العملية</summary>
    public DateTime OperationDate { get; set; }

    /// <summary>البيان / الوصف</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>معرف مركز التكلفة (اختياري)</summary>
    public int? CostCenterID { get; set; }

    /// <summary>معرف جهة الطرف المقابل (مورد/مقاول/جهة) - اختياري</summary>
    public int? CounterpartyAccountID { get; set; }

    /// <summary>معرف الجهة المرجعية الخارجية (للربط)</summary>
    public int? SourceReferenceID { get; set; }

    /// <summary>تفاصيل إضافية خاصة بأنواع معينة</summary>
    public string? ExtraData { get; set; }
}

/// <summary>
/// نتيجة تنفيذ عملية مالية
/// </summary>
public class FinancialOperationResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int JournalEntryID { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    /// <summary>تفاصيل بنود القيد المولّد</summary>
    public List<JournalEntryLineDto> GeneratedLines { get; set; } = new();
}

/// <summary>
/// طلب إدخال الأرصدة الافتتاحية
/// </summary>
public class InitialOpeningBalanceRequestDto
{
    /// <summary>معرف السنة المالية (يُحدد الفترة الأولى تلقائياً)</summary>
    public int FiscalYearID { get; set; }
    public DateTime AsOfDate { get; set; }
    public string Description { get; set; } = "إثبات الأرصدة الافتتاحية";

    /// <summary>قائمة بالحسابات وأرصدتها</summary>
    public List<OpeningBalanceLineDto> Lines { get; set; } = new();

    /// <summary>الأصول المضافة من صفحة الأرصدة الافتتاحية</summary>
    public List<OpeningBalanceAssetDto>? Assets { get; set; }
}

public class OpeningBalanceAssetDto
{
    public string AssetName { get; set; } = string.Empty;
    public string? AssetNumber { get; set; }
    public decimal AcquiredCost { get; set; }
    public int AssetAccountID { get; set; }
    public int CategoryID { get; set; }
    public int AssetTypeID { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal SalvageValue { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime PurchaseDate { get; set; }
    public DateTime InServiceDate { get; set; }
}

public class OpeningBalanceLineDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNature { get; set; } = string.Empty;
    /// <summary>الرصيد — موجب أو سالب. النظام يحدد الطرف (مدين/دائن) بناءً على الطبيعة والاشارة</summary>
    public decimal Balance { get; set; }
}

/// <summary>
/// استجابة جلب الأرصدة الافتتاحية الأولية لسنة مالية محددة
/// </summary>
public class InitialOpeningBalanceResponseDto
{
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public string? Description { get; set; }
    public int JournalEntryID { get; set; }
    public string? EntryNumber { get; set; }

    /// <summary>بنود الأرصدة الافتتاحية</summary>
    public List<OpeningBalanceLineDto> Lines { get; set; } = new();
}

// ═══════════════════════════════════════
// CASH FLOW STATEMENT (قائمة التدفقات النقدية)
// ═══════════════════════════════════════

public class CashFlowStatementDto
{
    public int FiscalYearID { get; set; }
    public string YearName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    // التدفقات التشغيلية
    public decimal OperatingCashInflow { get; set; }
    public decimal OperatingCashOutflow { get; set; }
    public decimal NetOperatingCashFlow => OperatingCashInflow - OperatingCashOutflow;

    // التدفقات الاستثمارية
    public decimal InvestingCashInflow { get; set; }
    public decimal InvestingCashOutflow { get; set; }
    public decimal NetInvestingCashFlow => InvestingCashInflow - InvestingCashOutflow;

    // التدفقات التمويلية
    public decimal FinancingCashInflow { get; set; }
    public decimal FinancingCashOutflow { get; set; }
    public decimal NetFinancingCashFlow => FinancingCashInflow - FinancingCashOutflow;

    // صافي التدفق النقدي
    public decimal NetCashFlow => NetOperatingCashFlow + NetInvestingCashFlow + NetFinancingCashFlow;

    // الأرصدة
    public decimal BeginningCashBalance { get; set; }
    public decimal EndingCashBalance => BeginningCashBalance + NetCashFlow;

    // التفاصيل
    public List<CashFlowItemDto> OperatingItems { get; set; } = new();
    public List<CashFlowItemDto> InvestingItems { get; set; } = new();
    public List<CashFlowItemDto> FinancingItems { get; set; } = new();
}

public class CashFlowItemDto
{
    public int AccountID { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "Inflow" or "Outflow"
}

// ═══════════════════════════════════════
// BANK TRANSACTION DTOs
// ═══════════════════════════════════════

public class BankTransactionDto
{
    public int TransactionID { get; set; }
    public int BankID { get; set; }
    public string? BankName { get; set; }
    public int JournalEntryID { get; set; }
    public string? EntryNumber { get; set; }
    public int LineID { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNo { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? BankStatementRef { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBankTransactionDto
{
    public int BankID { get; set; }
    public int? JournalEntryID { get; set; }
    public int? LineID { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNo { get; set; }
}

public class BankTransactionFilterDto
{
    public int? BankID { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsReconciled { get; set; }
}
