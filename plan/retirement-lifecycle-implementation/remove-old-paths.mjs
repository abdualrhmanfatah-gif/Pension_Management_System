import fs from 'node:fs';
function edit(p,fn){fs.writeFileSync(p,fn(fs.readFileSync(p,'utf8')).replace(/\r?\n/g,'\r\n'));}
edit('PensionMS.Infrastructure/Services/Pension/Files/FileStatusService.cs',s=>{
 s=s.replaceAll('    private readonly IWorkflowEngine _workflowEngine;\r\n','').replaceAll('    private readonly IRiskScoringService _riskScoringService;\r\n','').replaceAll('    private readonly ISnapshotService _snapshotService;\r\n','');
 s=s.replace('        IWorkflowEngine workflowEngine,\r\n','').replace('        IRiskScoringService riskScoringService,\r\n','').replace('        ISnapshotService snapshotService,\r\n','');
 s=s.replace('        _workflowEngine = workflowEngine;\r\n','').replace('        _riskScoringService = riskScoringService;\r\n','').replace('        _snapshotService = snapshotService;\r\n','');
 const a=s.indexOf('    /// <summary>\r\n    /// جلب أنواع الوثائق المطلوبة');
 if(a<0)throw new Error('guard cleanup anchor');
 return s.slice(0,a)+'}\n';
});
edit('PensionMS.Tests/Services/FileStatusServiceTests.cs',s=>s.replace('        Mock.Of<IWorkflowEngine>(), Mock.Of<IRiskScoringService>(), Mock.Of<IImmutableAuditService>(),\r\n        Mock.Of<ISnapshotService>(), Mock.Of<INotificationService>(), f.Service);','        Mock.Of<IImmutableAuditService>(), Mock.Of<INotificationService>(), f.Service);'));
edit('PensionMS.Infrastructure/Services/Security/SoD/SoDRules.cs',s=>{const a=s.indexOf('/// <summary>\r\n/// قاعدة SoD: المعتمِد');if(a<0)throw new Error('obsolete SoD rule');return s.slice(0,a);});
edit('PensionMS.Infrastructure/DependencyInjection/SecurityServiceCollectionExtensions.cs',s=>s.replace('        services.AddScoped<ISoDRule, ApproverCannotActivateRule>();\r\n',''));
edit('PensionMS.Tests/DependencyInjectionTests.cs',s=>s.replace('            typeof(ApproverCannotActivateRule),\r\n',''));
edit('PensionMS.Infrastructure/Services/Payroll/PayrollGenerationService.cs',s=>{const a=s.indexOf('        // Preload: FileIDs that have an active suspension'),b=s.indexOf('        // Preload: active recipients',a);if(a<0||b<0)throw new Error('obsolete suspension skip');return s.slice(0,a)+s.slice(b);});
