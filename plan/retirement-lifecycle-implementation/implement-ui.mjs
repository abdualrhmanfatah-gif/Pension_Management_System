import fs from 'node:fs';
const root='F:/Projects/Pension_Management_System/';
function edit(path,apply){const s=fs.readFileSync(root+path,'utf8'),backup=root+'plan/retirement-lifecycle-implementation/before/'+path;if(!fs.existsSync(backup)){fs.mkdirSync(backup.slice(0,backup.lastIndexOf('/')),{recursive:true});fs.writeFileSync(backup,s);}fs.writeFileSync(root+path,apply(s).replace(/\r?\n/g,'\r\n'));}
edit('ui/src/api/retirement.ts',s=>s.replace('    requestReview: (fileId: number) =>',`    reinstateEntitlement: (fileId: number) =>
        api.post<ApiResponse<boolean>>("/retirement-files/" + fileId + "/reinstate-entitlement").then(r => r.data),

    requestReview: (fileId: number) =>`));
edit('ui/src/constants/retirement.ts',s=>s.replace('    Closed: 4,','    EntitlementEnded: 4,\n    Closed: 4,').replace('export const CLOSED_STATUS_ID = FILE_STATUS.Closed;',`export const CLOSED_STATUS_ID = FILE_STATUS.EntitlementEnded;

export function retirementFileStatusName(statusId: number | null | undefined): string {
    switch (statusId) {
        case FILE_STATUS.New: return "جديد";
        case FILE_STATUS.Active: return "نشط";
        case FILE_STATUS.Pending: return "معلق";
        case FILE_STATUS.EntitlementEnded: return "انتهى استحقاقه";
        default: return "حالة قديمة — بانتظار التسوية";
    }
}`));
edit('ui/src/pages/retirement-files/FileReviewPage.tsx',s=>s.replace('retirementApi.updateFileStatus(fileId, 2, "اعتماد الملف")','retirementApi.approveReview(fileId)').replace('retirementApi.updateFileStatus(fileId, 3, rejectReason || "رفض الملف")','retirementApi.rejectReview(fileId, rejectReason)').replace('successMessage: "تم رفض الملف بنجاح"','successMessage: "رُفضت المراجعة؛ يبقى الملف معلقًا والصرف موقوفًا"'));
edit('ui/src/pages/retirement-files/tabs/ReviewTab.tsx',s=>s.replace(/            let newStatusId = 2;[\s\S]*?            return retirementApi.updateFileStatus\(fileId, newStatusId, notes\);/,`            if (action === REVIEW_ACTION.APPROVE) return retirementApi.approveReview(fileId, notes);
            return retirementApi.rejectReview(fileId, notes);`));
for(const path of ['ui/src/components/domain/retirement/FileReviewHeader.tsx','ui/src/components/domain/retirement/FileReviewSidebar.tsx']){
 edit(path,s=>s.replace('import { RecordStateBadge } from "@/components/ui";','import { Badge } from "@/components/ui";\nimport { retirementFileStatusName } from "@/constants/retirement";')
 .replace('<RecordStateBadge state={statusID as any} />','<Badge variant="outline">{retirementFileStatusName(statusID)}</Badge>')
 .replace('<RecordStateBadge state={(file.statusID as any) || "active"} />','<Badge variant="outline">{retirementFileStatusName(file.statusID)}</Badge>'));
}
edit('ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx',s=>{
 s='import { FileLifecycleActions } from "@/components/domain/retirement/FileLifecycleActions";\n'+s;
 return s.replace('            <div className="space-y-2.5 md:space-y-3.5">','            <div className="space-y-2.5 md:space-y-3.5">\n                <FileLifecycleActions fileId={fileId} statusId={file.statusID} reviewStatus={file.reviewStatus} />');
});
// Domain mutations can affect shared retirees and multiple file screens, balances and holds.
// Invalidating active observers after any API mutation avoids stale approval/payment controls.
edit('ui/src/hooks/useApiMutation.ts',s=>s.replace('            // 1. Invalidate Queries','            await queryClient.invalidateQueries();\n            // 1. Invalidate Queries'));
edit('ui/src/components/domain/retirement/StatusUpdateDialog.tsx',s=>{
 s=s.replace('ShieldAlert, ShieldBan, ShieldCheck, ShieldX','ShieldBan, ShieldCheck');
 s=s.replaceAll('[3, 4, 8]','[4]');
 const a=s.indexOf('const statusWarnings:'),b=s.indexOf('// ─── Component',a);
 s=s.slice(0,a)+`const statusWarnings: Record<number, { title: string; message: string; icon: React.ReactNode }> = {
    4: {
        title: "انتهى استحقاقه",
        message: "انتهت جميع الاستحقاقات المرتبطة بهذا الملف، مع الاحتفاظ ببياناته وسجله. إعادة الاستحقاق تتم على الملف نفسه عبر المراجعة والاعتماد.",
        icon: <ShieldBan className="w-5 h-5" />,
    },
    6: {
        title: "إرسال للمراجعة",
        message: "يصبح الملف معلقًا ويتوقف صرف المعاش حتى اعتماد المراجعة، مع استمرار تسجيل المبالغ المستحقة.",
        icon: <ShieldCheck className="w-5 h-5" />,
    },
};

`+s.slice(b);
 return s;
});
