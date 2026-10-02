const fs = require('node:fs');
const path = require('node:path');
const names = 'BirthDate|DateOfBirth|DeathDate|DeceasedDate|RetirementDate|RetirementDateFrom|RetirementDateTo|ServiceStartDate|ServiceEndDate|EnrollmentDate|IssueDate|ExpiryDate|NationalIDIssueDate|NationalIDExpiryDate|ActivePaymentBookExpiry|ApprovalRetirementDate|FileRetirementDate';
for (const root of ['PensionMS.Core', 'PensionMS.Infrastructure', 'PensionMS.API']) {
  for (const file of fs.readdirSync(root, {recursive:true}).filter(f => f.endsWith('.cs') && !/^(Migrations|obj|bin)[\\/]/.test(f))) {
    const p = path.join(root,file);
    const original=fs.readFileSync(p,'utf8');
    let s=original.replace(new RegExp('\\bDateTime(\\??)(\\s+(?:'+names+')\\s*\\{)', 'g'), 'DateOnly$1$2');
    s=s.replace(/BirthDate = new DateTime\((\d+), (\d+), (\d+), 0, 0, 0, DateTimeKind.Utc\)/g,'BirthDate = new DateOnly($1, $2, $3)');
    s=s.replace(/DateTime\.Today/g,'PensionMS.Core.Time.YemenTime.TodayDateTime');
    s=s.replace(/DateTime\.UtcNow:yyyy/g,'PensionMS.Core.Time.YemenTime.Now:yyyy');
    s=s.replace(/(ApprovalRetirementDate|FileRetirementDate)\?\.Date/g,'$1');
    s=s.replace(/(\w+\.ExpiryDate(?:!|\.Value)?) < DateTime.UtcNow/g,'$1 < PensionMS.Core.Time.YemenTime.Today');
    s=s.replace(/\(DateTime.UtcNow - (\w+).BirthDate\).TotalDays \/ 365.25/g,'PensionMS.Core.Time.YemenTime.Age($1.BirthDate)');
    s=s.replace(/ep.BirthDate.AddYears\(18\) > DateTime.UtcNow/g,'ep.BirthDate.AddYears(18) > PensionMS.Core.Time.YemenTime.Today');
    s=s.replace(/dto.DateOfBirth \?\? DateTime.UtcNow/g,'dto.DateOfBirth ?? PensionMS.Core.Time.YemenTime.Today');
    if(file.endsWith('MissingDocumentService.cs')) s=s.replace('var today = DateTime.UtcNow;', 'var today = PensionMS.Core.Time.YemenTime.Today;').replace('ep.BirthDate.Date', 'ep.BirthDate');
    if(file.endsWith('PaymentBookService.cs')) s=s.replace(/DateTime.UtcNow.AddDays/g,'PensionMS.Core.Time.YemenTime.Today.AddDays').replace('(int?)(pb.ExpiryDate - DateTime.UtcNow).TotalDays','(int?)(pb.ExpiryDate.DayNumber - PensionMS.Core.Time.YemenTime.Today.DayNumber)').replace('var year = DateTime.UtcNow.Year;', 'var year = PensionMS.Core.Time.YemenTime.Today.Year;');
    s=s.replace(/DateTime deathDate/g,'DateOnly deathDate');
    s=s.replace(/DateTime\? retirementDate(From|To)/g,'DateOnly? retirementDate$1');
    if(file.endsWith('RetirementFileService.cs')) {
      s=s.replace(/var today = PensionMS.Core.Time.YemenTime.TodayDateTime;\s*DateTime periodFrom;[\s\S]*?query = query.Where\(f => f.Documents.Any\(d => d.CreatedAt >= periodFrom && d.CreatedAt <= periodTo\)\);/,
        'var (periodFrom, periodTo) = PensionMS.Core.Time.YemenTime.PeriodUtc(period);\n            query = query.Where(f => f.Documents.Any(d => d.CreatedAt >= periodFrom && d.CreatedAt < periodTo));');
      s=s.replace(/if \(filter.Period == "today"\)[\s\S]*?else if \(filter.FromDate.HasValue \|\| filter.ToDate.HasValue\)/g,
        'if (filter.Period is "today" or "week" or "month" or "year")\n        {\n            var (start, end) = PensionMS.Core.Time.YemenTime.PeriodUtc(filter.Period);\n            query = query.Where(d => d.CreatedAt >= start && d.CreatedAt < end);\n        }\n        else if (filter.FromDate.HasValue || filter.ToDate.HasValue)');
      s=s.replace(/if \(filter.FromDate.HasValue\)\s*query = query.Where\(([df]) => \1.CreatedAt >= filter.FromDate.Value\);/g,
        'if (filter.FromDate.HasValue)\n            {\n                var start = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(filter.FromDate.Value));\n                query = query.Where($1 => $1.CreatedAt >= start);\n            }');
      s=s.replace(/if \(filter.ToDate.HasValue\)\s*query = query.Where\(([df]) => \1.CreatedAt <= filter.ToDate.Value.AddDays\(1\).AddTicks\(-1\)\);/g,
        'if (filter.ToDate.HasValue)\n            {\n                var end = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(filter.ToDate.Value).AddDays(1));\n                query = query.Where($1 => $1.CreatedAt < end);\n            }');
    }
    s=s.replace('ep.StopDate     = dto.DeathDate;', 'ep.StopDate     = dto.DeathDate.ToDateTime(TimeOnly.MinValue);');
    s=s.replace('r.EndDate = dto.IssueDate;', 'r.EndDate = dto.IssueDate.ToDateTime(TimeOnly.MinValue);');
    s=s.replace('StartDate = dto.IssueDate,', 'StartDate = dto.IssueDate.ToDateTime(TimeOnly.MinValue),');
    s=s.replace(/retiree.ApprovalItem.RetirementDate \?\? retiree.ApprovalItem.CreatedAt/g,'retiree.ApprovalItem.RetirementDate?.ToDateTime(TimeOnly.MinValue) ?? PensionMS.Core.Time.YemenTime.LocalTime(new DateTimeOffset(DateTime.SpecifyKind(retiree.ApprovalItem.CreatedAt, DateTimeKind.Utc))).Date');
    s=s.replace('file.RetirementDate ?? DateTime.UtcNow', 'file.RetirementDate?.ToDateTime(TimeOnly.MinValue) ?? PensionMS.Core.Time.YemenTime.TodayDateTime');
    s=s.replace('(int)(DateTime.UtcNow - d.ExpiryDate!.Value).TotalDays','PensionMS.Core.Time.YemenTime.Today.DayNumber - d.ExpiryDate!.Value.DayNumber');
    if(file.endsWith('MinistryApprovalService.cs')) {
      s=s.replace('private DateTime? ParseDate(', 'private DateOnly? ParseDate(')
        .replace(/return DateTime.FromOADate\((d|oa)\);/g,'return DateOnly.FromDateTime(DateTime.FromOADate($1));')
        .replace(/return dt;/g,'return DateOnly.FromDateTime(dt);');
    }
    if(s!==original)fs.writeFileSync(p,s);
  }
}
