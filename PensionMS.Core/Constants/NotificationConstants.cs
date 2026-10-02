namespace PensionMS.Core.Constants;

public static class NotificationConstants
{
    public static class Categories
    {
        public const string Security = "SECURITY";
        public const string Financial = "FINANCIAL";
        public const string Workflow = "WORKFLOW";
        public const string System = "SYSTEM";
        public const string HR = "HR";
        public const string Pensions = "PENSIONS";
    }

    public static class Types
    {
        public const string Info = "INFO";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
        public const string Success = "SUCCESS";
    }

    public static class Priorities
    {
        public const byte Low = 0;
        public const byte Normal = 1;
        public const byte High = 2;
        public const byte Critical = 3;
    }
}
