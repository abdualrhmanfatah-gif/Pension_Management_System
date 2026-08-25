namespace PensionMS.Core.Entities.Accounting;

public enum SubledgerType
{
    Account = 4
}

public enum AccountCategory
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum BankTransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Fee = 4
}
