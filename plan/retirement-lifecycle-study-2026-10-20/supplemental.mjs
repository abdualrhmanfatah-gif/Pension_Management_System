// Cross-check indirect writers and Arabic labels not covered by the narrow symbol search.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const out=path.dirname(fileURLToPath(import.meta.url)),root=path.resolve(out,'../..');
const read=n=>JSON.parse(fs.readFileSync(path.join(out,n),'utf8'));
const manifest=read('source-manifest.json'),ledger=read('coverage-ledger.json'),methods=read('methods.json');
const labels=/موقوف|معلق|مرفوض|ناقص|انتهى استحقاق|FileStatus|fileStatus|file\.status|file\.Status/;
const domain=/\b(?:RetirementFile|Retiree|EntitledPerson|Agent|Guardian|GuardianEntitledPerson|FinancialEntitlement|PensionSuspension|SalaryRecipient|PaymentBook|DocumentPledge|DocumentExemption|MissingDocument)s?\b/;
const refs=[],writers=[];
for(const item of manifest.filter(x=>x.area==='runtime-source')){
  const lines=fs.readFileSync(path.join(root,item.path),'utf8').split(/\r?\n/);
  lines.forEach((text,i)=>{if(labels.test(text))refs.push({path:item.path,line:i+1,text:text.trim()});});
  for(const m of methods.filter(m=>m.path===item.path&&m.save)){
    const body=lines.slice(m.line-1,m.end).join('\n');
    if(domain.test(body))writers.push(m);
  }
}
fs.writeFileSync(path.join(out,'supplemental-references.json'),JSON.stringify(refs,null,2)+'\n','utf8');
fs.writeFileSync(path.join(out,'indirect-writers.json'),JSON.stringify(writers,null,2)+'\n','utf8');
const unexpected=ledger.filter(x=>x.area==='runtime-source'&&x.disposition==='OUT_OF_SCOPE'&&(refs.some(r=>r.path===x.path)||writers.some(w=>w.path===x.path))).map(x=>x.path);
console.log(JSON.stringify({label_references:refs.length,writer_methods:writers.length,unclassified_candidates:unexpected},null,2));
