using System.Linq.Expressions;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Repositories;

/// <summary>One normalization definition used by both CLR and the translated SQL predicate.</summary>
internal static class SettlementMilitaryNumber
{
    private static readonly (string From, string To)[] Replacements = CreateReplacements();

    private static (string, string)[] CreateReplacements()
    {
        var replacements = new List<(string, string)>();
        for (var digit = 0; digit < 10; digit++)
        {
            replacements.Add((((char)('\u0660' + digit)).ToString(), digit.ToString()));
            replacements.Add((((char)('\u06f0' + digit)).ToString(), digit.ToString()));
        }
        // Whitespace removal preserves leading zeroes and does not parse an identifier as a number.
        foreach (var c in new[] { ' ', '\t', '\r', '\n', '\u00a0' })
            replacements.Add((c.ToString(), ""));
        return replacements.ToArray();
    }

    public static string Normalize(string? value)
    {
        var result = value ?? "";
        foreach (var (from, to) in Replacements) result = result.Replace(from, to);
        return result;
    }

    public static Expression<Func<SettlementItem, bool>> Matches(int retireeId, int approvalId, string number)
    {
        Expression<Func<SettlementItem, bool>> linked = i => i.RetireeID == retireeId
            || approvalId > 0 && i.ApprovalItemID == approvalId;
        if (number.Length == 0) return linked;
        var parameter = linked.Parameters[0];
        Expression<Func<SettlementItem, string?>> raw = i => i.MilitaryNumber;
        Expression<Func<SettlementItem, string?>> approved = i => i.ApprovalItem == null ? null : i.ApprovalItem.MilitaryNumber;
        Expression NormalizeExpression(Expression expression)
        {
            Expression result = Expression.Coalesce(expression, Expression.Constant(""));
            foreach (var (from, to) in Replacements)
                result = Expression.Call(result, nameof(string.Replace), Type.EmptyTypes,
                    Expression.Constant(from), Expression.Constant(to));
            return Expression.Equal(result, Expression.Constant(number));
        }
        var rawBody = new ReplaceParameter(raw.Parameters[0], parameter).Visit(raw.Body)!;
        var approvedBody = new ReplaceParameter(approved.Parameters[0], parameter).Visit(approved.Body)!;
        return Expression.Lambda<Func<SettlementItem, bool>>(
            Expression.OrElse(linked.Body, Expression.OrElse(NormalizeExpression(rawBody), NormalizeExpression(approvedBody))), parameter);
    }

    private sealed class ReplaceParameter(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : node;
    }
}
