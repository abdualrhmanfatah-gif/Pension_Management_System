import fs from 'node:fs';
import path from 'node:path';
import assert from 'node:assert/strict';

const directory='plan/code-cleanup-2026-10-20';
const before=JSON.parse(fs.readFileSync(directory+'/lint-before.json','utf8'));
const after=JSON.parse(fs.readFileSync(directory+'/lint-after.json','utf8'));
const normalize=file=>path.relative(process.cwd(),file).replaceAll('\\','/').replace(/pages\/correspondence\/(?:Incoming|Outgoing)RegisterPage\.tsx$/,'pages/correspondence/CorrespondenceRegisterPage.tsx');
const key=(file,m)=>JSON.stringify([normalize(file),m.ruleId,m.message,m.severity]);
const remaining=new Map();
for(const file of before)for(const message of file.messages){const k=key(file.filePath,message);remaining.set(k,(remaining.get(k)??0)+1);}
const added=[];
for(const file of after)for(const message of file.messages){const k=key(file.filePath,message),n=remaining.get(k)??0;if(n)remaining.set(k,n-1);else added.push({file:normalize(file.filePath),...message});}
const counts=list=>({errors:list.reduce((n,f)=>n+f.errorCount,0),warnings:list.reduce((n,f)=>n+f.warningCount,0)});
const result={before:counts(before),after:counts(after),newProblems:added.length,added};
fs.writeFileSync(directory+'/lint-comparison.json',JSON.stringify(result,null,2)+'\n');
console.log(JSON.stringify(result,null,2));
assert.equal(added.length,0,'Cleanup introduced new lint problems.');
