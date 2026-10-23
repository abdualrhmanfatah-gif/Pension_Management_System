// Verify study coverage, evidence anchors, and integrity of the original source snapshot.
import fs from 'node:fs';
import path from 'node:path';
import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
const out = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(out, '../..');
const read = n=>JSON.parse(fs.readFileSync(path.join(out,n),'utf8'));
const manifest=read('source-manifest.json'),ledger=read('coverage-ledger.json'),refs=read('references.json'),sites=read('change-sites.json'),mutations=read('domain-mutations.json'),supplemental=read('supplemental-references.json');
const failures=[],changed=[],missing=[],source=new Map();
for(const item of manifest){
  const full=path.join(root,item.path);
  if(!fs.existsSync(full)){missing.push(item.path);continue;}
  const bytes=fs.readFileSync(full);if(createHash('sha256').update(bytes).digest('hex')!==item.sha256)changed.push(item.path);
  source.set(item.path,bytes.toString('utf8').split(/\r?\n/));
}
if(new Set(ledger.map(x=>x.path)).size!==manifest.length||ledger.length!==manifest.length)failures.push('Coverage does not match source snapshot');
for(const s of sites)for(const a of s.sites)if(!source.get(s.path)?.[a.line-1]?.includes(a.anchor))failures.push(`Invalid anchor ${s.path}:${a.line}`);
for(const r of refs)if(!source.has(r.path)||r.line<1||r.line>source.get(r.path).length||!source.get(r.path)[r.line-1].trim().startsWith(r.text))failures.push(`Invalid reference ${r.path}:${r.line}`);
for(const r of supplemental)if(source.get(r.path)?.[r.line-1]?.trim()!==r.text)failures.push(`Invalid supplemental reference ${r.path}:${r.line}`);
for(const m of mutations)if(!source.get(m.path)?.[m.line-1]?.includes(m.name))failures.push(`Invalid method ${m.path}:${m.line}`);
const report=fs.readFileSync(path.join(root,'plan/feature-context-retirement-lifecycle.md'),'utf8');
for(const m of report.matchAll(/\]\(([^)]+)\)/g)){if(!/^(?:https?:|#)/.test(m[1])&&!fs.existsSync(path.resolve(root,'plan',m[1])))failures.push('Missing report link '+m[1]);}
const result={snapshot_files:manifest.length,coverage_files:ledger.length,validated_anchors:sites.reduce((n,s)=>n+s.sites.length,0),validated_references:refs.length,validated_supplemental_references:supplemental.length,validated_mutation_candidates:mutations.length,changed_existing_files:changed,missing_existing_files:missing,failures};
fs.writeFileSync(path.join(out,'validation.json'),JSON.stringify(result,null,2)+'\n','utf8');console.log(JSON.stringify(result));
if(failures.length||changed.some(p=>/^(?:PensionMS\.(?:Core|Infrastructure|API)\/|ui\/src\/)/.test(p))||missing.length)process.exitCode=1;
