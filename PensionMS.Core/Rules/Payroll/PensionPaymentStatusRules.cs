using PensionMS.Core.Constants;

namespace PensionMS.Core.Rules.Payroll;

public static class PensionPaymentStatusRules
{
    public enum MutationFailure { None, AlreadyReceived, InvalidTransition, SentAmountOrRecipientChanged }

    public static MutationFailure EvaluateMutation(bool isModification, int? originalStatus, int? nextStatus,
        decimal originalAmount, decimal amount, int? originalRecipientId, int? recipientId)
    {
        if (isModification && originalStatus == TransactionStatusConstants.Received)
            return MutationFailure.AlreadyReceived;
        if (isModification && !CanChange(originalStatus, nextStatus))
            return MutationFailure.InvalidTransition;
        if (originalStatus == TransactionStatusConstants.Sent &&
            (originalAmount != amount || originalRecipientId != recipientId))
            return MutationFailure.SentAmountOrRecipientChanged;
        return MutationFailure.None;
    }

    public static bool CanChange(int? current, int? next) => current == next || current switch
    {
        TransactionStatusConstants.PendingSend => next is TransactionStatusConstants.Sent or
            TransactionStatusConstants.Received or TransactionStatusConstants.Failed or TransactionStatusConstants.Cancelled,
        TransactionStatusConstants.Sent => next is TransactionStatusConstants.Received or TransactionStatusConstants.Failed,
        TransactionStatusConstants.Failed => next == TransactionStatusConstants.PendingSend,
        _ => false
    };
}
