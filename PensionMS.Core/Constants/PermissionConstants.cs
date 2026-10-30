namespace PensionMS.Core.Constants;

public static class PermissionConstants
{
<<<<<<< HEAD
    public const string FileTransfer = "File.Transfer";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    // ── Agent ──
    public const string AgentView = "Agent.View";
    public const string AgentAdd = "Agent.Add";
    public const string AgentEdit = "Agent.Edit";
    public const string AgentManage = "Agent.Manage";
    public const string AgentDelete = "Agent.Delete";

    // ── EntitledPerson ──
    public const string EntitledPersonView = "EntitledPerson.View";
    public const string EntitledPersonCreate = "EntitledPerson.Create";
    public const string EntitledPersonEdit = "EntitledPerson.Edit";
    public const string EntitledPersonDelete = "EntitledPerson.Delete";
    public const string EntitledPersonLifecycle = "EntitledPerson.Lifecycle";

    // ── Guardian ──
    public const string GuardianView = "Guardian.View";
    public const string GuardianAdd = "Guardian.Add";
    public const string GuardianEdit = "Guardian.Edit";
    public const string GuardianManage = "Guardian.Manage";
    public const string GuardianDelete = "Guardian.Delete";

    // ── Retiree ──
    public const string RetireeView = "Retiree.View";
    public const string RetireeEdit = "Retiree.Edit";
    public const string RetireeDelete = "Retiree.Delete";

    // ── Documents ──
    public const string FileDocumentsUpload = "File.Documents.Upload";
    public const string FileDocumentsUpdate = "File.Documents.Update";

    // ── DocumentExemption ──
    public const string DocumentExemptionView = "DocumentExemption.View";
    public const string DocumentExemptionAdd = "DocumentExemption.Add";
    public const string DocumentExemptionEdit = "DocumentExemption.Edit";
    public const string DocumentExemptionManage = "DocumentExemption.Manage";
    public const string DocumentExemptionDelete = "DocumentExemption.Delete";

    // ── DocumentPledge ──
    public const string DocumentPledgeView = "DocumentPledge.View";
    public const string DocumentPledgeAdd = "DocumentPledge.Add";
    public const string DocumentPledgeEdit = "DocumentPledge.Edit";
    public const string DocumentPledgeManage = "DocumentPledge.Manage";
    public const string DocumentPledgeDelete = "DocumentPledge.Delete";

    // ── MissingDocument ──
    public const string MissingDocumentView = "MissingDocument.View";
    public const string MissingDocumentAdd = "MissingDocument.Add";
    public const string MissingDocumentEdit = "MissingDocument.Edit";
    public const string MissingDocumentManage = "MissingDocument.Manage";
    public const string MissingDocumentDelete = "MissingDocument.Delete";

    // ── Missing Delete Permissions ──
    public const string PaymentBookDelete = "PaymentBook.Delete";
    public const string SalaryRecipientDelete = "SalaryRecipient.Delete";
    public const string SuspensionDelete = "Suspension.Delete";

    // ── Missing Add/Delete Permissions ──
    public const string FileNoteAdd = "FileNote.Add";
    public const string FileNoteDelete = "FileNote.Delete";
    public const string SettlementAdd = "Settlement.Add";
    public const string SettlementDelete = "Settlement.Delete";
    public const string MinistryAdd = "Ministry.Add";
    public const string MinistryDelete = "Ministry.Delete";

    // ── Investment Granular ──
    public const string InvestmentAdd = "Investment.Add";
    public const string InvestmentEdit = "Investment.Edit";
    public const string InvestmentDelete = "Investment.Delete";

    // ── Payroll Action Specific ──
    public const string PayrollExcludeBeneficiary = "Payroll.ExcludeBeneficiary";
    public const string PayrollManageDeductions = "Payroll.ManageDeductions";

    // ── Granular System/Lookup Permissions ──
    public const string SystemSettingsBanks = "System.Settings.Banks";
    public const string SystemSettingsDocumentTypes = "System.Settings.DocumentTypes";
    public const string SystemSettingsJobTitles = "System.Settings.JobTitles";
    public const string SystemSettingsInvestmentTypes = "System.Settings.InvestmentTypes";

    // ── Unified History View ──
    public const string FileViewHistory = "File.View.History";

    // ── Financial Advanced & Reporting ──
    public const string FinancialReportsView = "Financial.Reports.View";
    public const string FinancialBankStatementView = "Financial.Bank.Statement.View";
    public const string SystemAuditExport = "System.Audit.Export";

    // ── HR Module ──
    public const string HrDashboardView = "HR.Dashboard.View";
    public const string HrReferenceView = "HR.Reference.View";
    public const string HrReferenceManage = "HR.Reference.Manage";
    public const string HrEmployeeView = "HR.Employee.View";
    public const string HrEmployeeCreate = "HR.Employee.Create";
    public const string HrEmployeeEdit = "HR.Employee.Edit";
    public const string HrEmployeeDelete = "HR.Employee.Delete";
    public const string HrServiceHistoryView = "HR.ServiceHistory.View";

    // ── Inventory Module ──
    public const string InventoryItemView = "Inventory.Item.View";
    public const string InventoryItemCreate = "Inventory.Item.Create";
    public const string InventoryItemEdit = "Inventory.Item.Edit";
    public const string InventoryItemDelete = "Inventory.Item.Delete";
    public const string InventoryCustodyView = "Inventory.Custody.View";
    public const string InventoryCustodyCreate = "Inventory.Custody.Create";
    public const string InventoryCustodyReturn = "Inventory.Custody.Return";
    public const string InventoryTransactionView = "Inventory.Transaction.View";
    public const string InventoryTransactionCreate = "Inventory.Transaction.Create";
    public const string InventoryStocktake = "Inventory.Stocktake";

    // File Review
    public const string FileReviewView = "File.Review.View";
    public const string FileReviewApprove = "File.Review.Approve";
    public const string FileReviewReject = "File.Review.Reject";

    // ── Accounting Module ──
    public const string AccountingView = "Accounting.View";
    public const string AccountingManage = "Accounting.Manage";
    public const string AccountingReports = "Accounting.Reports";
}
