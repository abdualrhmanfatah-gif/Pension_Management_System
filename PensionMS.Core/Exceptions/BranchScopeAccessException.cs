namespace PensionMS.Core.Exceptions;

public sealed class BranchScopeAccessException : Exception
{
    public BranchScopeAccessException() : base("لا يسمح نطاق الفرع الحالي بالوصول إلى هذه البيانات") { }
}
