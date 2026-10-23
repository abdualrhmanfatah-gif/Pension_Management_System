namespace PensionMS.Core.Constants;

public static class PensionPaymentStatusRules
{
    public static bool CanChange(int? current, int? next) => current == next || current switch
    {
        TransactionStatusConstants.PendingSend => next is TransactionStatusConstants.Sent or
            TransactionStatusConstants.Received or TransactionStatusConstants.Failed or TransactionStatusConstants.Cancelled,
        TransactionStatusConstants.Sent => next is TransactionStatusConstants.Received or TransactionStatusConstants.Failed,
        TransactionStatusConstants.Failed => next == TransactionStatusConstants.PendingSend,
        _ => false
    };
}
