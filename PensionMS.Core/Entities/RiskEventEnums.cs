namespace PensionMS.Core.Entities;

public enum RiskSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum RiskStatus
{
    Open = 0,
    Investigating = 1,
    Resolved = 2
}

public enum RiskType
{
    HighValueRetirement = 0,
    MultipleEdits = 1,
    AfterHoursAction = 2,
    SuspiciousSoD = 3
}
