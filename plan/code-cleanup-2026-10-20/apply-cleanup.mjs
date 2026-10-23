import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { execFileSync } from 'node:child_process';

const root = fs.realpathSync(process.cwd());
const output = path.join(root, 'plan/code-cleanup-2026-10-20');
const backup = path.join(root, '.artifacts/code-cleanup-2026-10-20/original');
const ts = createRequire(path.join(root, 'ui/package.json'))('typescript');
const front = JSON.parse(fs.readFileSync('plan/file-audit-2026-10-20/frontend.json', 'utf8'));
const sha256 = data => crypto.createHash('sha256').update(data).digest('hex');
const uiFiles = front.dead.map(x => x.file);
assert.equal(uiFiles.length, 33);
const backendFiles = [
  'PensionMS.Infrastructure/Services/Citizen/CitizenAuthService.cs',
  'PensionMS.Infrastructure/Services/Citizen/CitizenDataService.cs',
  'PensionMS.Infrastructure/Services/Citizen/CitizenServicePortal.cs',
  'PensionMS.Infrastructure/Services/Citizen/CitizenSupportService.cs',
  'PensionMS.Infrastructure/Jobs/PledgeExpiryCheckerService.cs',
  'PensionMS.Infrastructure/Data/EncryptedValueConverter.cs',
  'PensionMS.Infrastructure/Data/Seeders/JournalEntryTypeSeeder.cs',
  'PensionMS.Core/Helpers/DataMasking.cs',
  'PensionMS.Core/Constants/DocumentStatus.cs',
  'PensionMS.Core/Constants/LeaseStatus.cs',
  'PensionMS.Core/Constants/FileReviewStatusConstants.cs',
  'PensionMS.Core/Interfaces/ICitizenAuthService.cs',
  'PensionMS.Core/Interfaces/ICitizenDataService.cs',
  'PensionMS.Core/Interfaces/ICitizenServicePortal.cs',
  'PensionMS.Core/Interfaces/ICitizenSupportService.cs',
  'PensionMS.Core/DTOs/Citizen/CitizenAuthDtos.cs',
  'PensionMS.Core/DTOs/Citizen/CitizenDataDtos.cs',
  'PensionMS.Core/DTOs/Citizen/CitizenServiceDtos.cs',
  'PensionMS.Core/DTOs/Citizen/CitizenSupportDtos.cs',
];
const assetFiles = front.sourceAssetRefs.filter(x => !x.references.length).map(x => x.file);
assert.equal(assetFiles.length, 8);
const archivedScripts = ['patch.py', 'patch_perf.py'];
const removed = [...uiFiles, ...backendFiles, ...assetFiles, ...archivedScripts];
const changed = [
  'PensionMS.Tests/DependencyInjectionTests.cs',
  'ui/src/pages/correspondence/IncomingRegisterPage.tsx',
  'ui/src/pages/correspondence/OutgoingRegisterPage.tsx',
  'ui/src/pages/retirement-files/DocumentProcessPage.tsx',
  'ui/src/components/domain/retirement/DocumentProcessDialog.tsx',
  'ui/src/components/ui/ImageCropperDialog.tsx',
  'ui/package.json', 'ui/package-lock.json',
];
const removalSet = new Set(removed);
assert.equal(removed.length, removalSet.size);
for (const edge of front.specifiers) {
  assert.ok(!removalSet.has(edge.target) || removalSet.has(edge.from), `Retained importer: ${edge.from} -> ${edge.target}`);
}
assert.ok(!fs.existsSync(backup), 'Backup already exists; do not rerun mutations.');
const checkedPath = file => {
  const resolved = fs.realpathSync(path.resolve(root, file));
  assert.ok(resolved.startsWith(root + path.sep), `Outside workspace: ${file}`);
  assert.ok(!file.includes('uploads/'), 'Uploaded data is outside this cleanup.');
  return resolved;
};
const records = [...removed, ...changed].map(file => {
  const resolved = checkedPath(file);
  const data = fs.readFileSync(resolved);
  return {file, bytes:data.length, sha256:sha256(data), action:removalSet.has(file) ? archivedScripts.includes(file) ? 'archive' : 'remove' : 'modify'};
});
for (const record of records) {
  const target = path.join(backup,record.file);
  fs.mkdirSync(path.dirname(target),{recursive:true});
  fs.copyFileSync(checkedPath(record.file),target,fs.constants.COPYFILE_EXCL);
  assert.equal(sha256(fs.readFileSync(target)), record.sha256);
}
fs.writeFileSync(path.join(output,'git-status-before.txt'),execFileSync('git',['status','--porcelain=v1']));
fs.writeFileSync(path.join(output,'manifest.json'),JSON.stringify({backup,files:records},null,2)+'\n');
// No recursive removals. Each deleted file has a verified, byte-identical backup.
for (const file of removed) fs.unlinkSync(checkedPath(file));

// Preserve the test's negative registration assertion without retaining deleted types.
const testPath = 'PensionMS.Tests/DependencyInjectionTests.cs';
let test = fs.readFileSync(testPath,'utf8');
assert.ok(test.includes('typeof(PledgeExpiryCheckerService)'));
test = test.replace('s.ImplementationType == typeof(PledgeExpiryCheckerService)', 's.ImplementationType?.Name == "PledgeExpiryCheckerService"');
test = test.replace(/s\.ServiceType == typeof\((ICitizen\w+)\)/g, (_,name)=>`s.ServiceType.FullName == "PensionMS.Core.Interfaces.${name}"`);
fs.writeFileSync(testPath,test);

// One shared correspondence page; preserve every existing label and route wrapper.
const incomingPath = 'ui/src/pages/correspondence/IncomingRegisterPage.tsx';
let incoming = fs.readFileSync(incomingPath,'utf8');
const outgoing = fs.readFileSync('ui/src/pages/correspondence/OutgoingRegisterPage.tsx','utf8');
const commonPath = 'ui/src/pages/correspondence/CorrespondenceRegisterPage.tsx';
assert.ok(!fs.existsSync(commonPath));
incoming = incoming.replace('  FileDown,','  FileDown,\n  FileUp,');
incoming = incoming.replace('/**\r\n * صفحة سجل الوارد\r\n */\r\nexport function IncomingRegisterPage() {', 'interface CorrespondenceRegisterPageProps {\n  direction: CorrespondenceDirection;\n}\n\nexport function CorrespondenceRegisterPage({ direction }: CorrespondenceRegisterPageProps) {\n  const incoming = direction === CorrespondenceDirection.Incoming;\n  const Icon = incoming ? FileDown : FileUp;');
assert.ok(!incoming.includes('export function IncomingRegisterPage'));
incoming = incoming.replace('  const direction = CorrespondenceDirection.Incoming;\r\n','');
incoming = incoming.replace('title: "حذف قيد وارد",','title: incoming ? "حذف قيد وارد" : "حذف قيد صادر",');
incoming = incoming.replace('[confirm, deleteMutation]);','[confirm, deleteMutation, incoming]);');
incoming = incoming.replace('title="سجل الوارد"','title={incoming ? "سجل الوارد" : "سجل الصادر"}');
incoming = incoming.replace('description="توثيق وأرشفة جميع المراسلات والمعاملات الواردة للمؤسسة"','description={incoming ? "توثيق وأرشفة جميع المراسلات والمعاملات الواردة للمؤسسة" : "توثيق وأرشفة جميع المراسلات والأوامر الصادرة من المؤسسة"}');
incoming = incoming.replace('createLabel="تسجيل وارد"','createLabel={incoming ? "تسجيل وارد" : "تسجيل صادر"}');
incoming = incoming.replace('icon={<FileDown ','icon={<Icon ');
fs.writeFileSync(commonPath,incoming.replaceAll('\r\n','\n'));
for (const [name,direction] of [['IncomingRegisterPage','Incoming'],['OutgoingRegisterPage','Outgoing']]) {
  const wrapper=`import { CorrespondenceDirection } from "@/types/correspondence";\n\nimport { CorrespondenceRegisterPage } from "./CorrespondenceRegisterPage";\n\nexport function ${name}() {\n  return <CorrespondenceRegisterPage direction={CorrespondenceDirection.${direction}} />;\n}\n`;
  fs.writeFileSync(`ui/src/pages/correspondence/${name}.tsx`,wrapper);
}
assert.ok(outgoing.includes('المراسلات والأوامر الصادرة من المؤسسة'));

// Extract identical, stateless image helpers. Keep each consumer's canvas pool
// and its retention limit in that consumer (the page and dialog differ).
const helperPath = 'ui/src/lib/document-image.ts';
assert.ok(!fs.existsSync(helperPath));
const dialogPath='ui/src/components/domain/retirement/DocumentProcessDialog.tsx';
const functions=['rotateCorners','canvasToFile','applySharpen','renderSettingsImage'];
const dialog=fs.readFileSync(dialogPath,'utf8');
const sourceFile = ts.createSourceFile(dialogPath,dialog,ts.ScriptTarget.Latest,true);
const helperParts = sourceFile.statements.filter(node => ts.isInterfaceDeclaration(node) && node.name.text==='ImageSettings' || ts.isFunctionDeclaration(node) && functions.includes(node.name?.text));
assert.equal(helperParts.length,5);
fs.writeFileSync(helperPath,helperParts.map(node=>'export '+node.getText(sourceFile)).join('\n\n').replaceAll('\r\n','\n')+'\n');
for (const file of ['ui/src/pages/retirement-files/DocumentProcessPage.tsx',dialogPath,'ui/src/components/ui/ImageCropperDialog.tsx']) {
  let text=fs.readFileSync(file,'utf8');
  const ast=ts.createSourceFile(file,text,ts.ScriptTarget.Latest,true);
  const removedNodes=ast.statements.filter(node => ts.isInterfaceDeclaration(node) && node.name.text==='ImageSettings' || ts.isFunctionDeclaration(node) && functions.includes(node.name?.text));
  for (const node of [...removedNodes].sort((a,b)=>b.pos-a.pos)) text=text.slice(0,node.getStart(ast))+text.slice(node.end);
  const names=file.includes('ImageCropperDialog') ? 'canvasToFile' : 'applySharpen, canvasToFile, type ImageSettings, renderSettingsImage, rotateCorners';
  text=`import { ${names} } from "@/lib/document-image";\n`+text;
  fs.writeFileSync(file,text.replaceAll('\r\n','\n').replace(/\n{3,}/g,'\n\n'));
}
console.log(JSON.stringify({removed:removed.length,uiFiles:uiFiles.length,backendFiles:backendFiles.length,assets:assetFiles.length,archivedScripts:archivedScripts.length,backup,created:[commonPath,helperPath]},null,2));
