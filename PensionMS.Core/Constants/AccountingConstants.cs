namespace PensionMS.Core.Constants;

public static class AccountingConstants
{
    public static class AccountCodes
    {
        public const string Treasury = "1811";
        public const string AccruedSalaries = "273";
        public const string DeferredRent = "271";
        public const string DeferredGrant = "272";
        public const string IncomeCredit = "2814";
        public const string IncomeDebit = "2812";
        public const string Profit = "2222";
        public const string Loss = "2221";
        public const string LegalReserve = "221";
        public const string GeneralReserve = "222";
        public const string RevenueLegalReserve = "2821";
        public const string RevenueGeneralReserve = "2822";
        public const string SecurityDeposit = "511";
        public const string SecurityRefund = "521";
    }

    public static class CashFlowPrefixes
    {
        public const string Investing = "11";
        public const string FinancingEquity = "21";
        public const string FinancingLiability = "22";
    }

    public static class BookNumbers
    {
        public const int Assets = 1;
        public const int Liabilities = 2;
        public const int Expenses = 3;
        public const int Revenues = 4;
        public const int Statutory = 5;
    }
}
