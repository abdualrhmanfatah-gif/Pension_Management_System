// Derive method and frontend dependency inventories from the study's source manifest.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const out = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(out, '../..');
const manifest = JSON.parse(fs.readFileSync(path.join(out, 'source-manifest.json'), 'utf8'));
const methods = [], imports = [];
for (const item of manifest) {
  const source = fs.readFileSync(path.join(root, item.path), 'utf8');
  const lines = source.split(/\r?\n/);
  if (item.area !== 'historical-artifact' && item.path.endsWith('.cs') && /(?:Services|Controllers|Helpers|Interceptors|Jobs|Repositories|Data\/UnitOfWorkTransaction)/.test(item.path)) {
    const starts = [];
    for (let i = 0; i < lines.length; i++) {
      const match = lines[i].match(/^\s*(public|private|protected|internal)\s+(?:static\s+)?(?:async\s+)?[^=;]+?\b(\w+(?:Async)?)\s*\(/);
      if (match && !/^(if|switch|catch|foreach|while)$/.test(match[2])) starts.push({ line: i + 1, name: match[2], access: match[1] });
    }
    starts.forEach((start, i) => {
      const end = (starts[i + 1]?.line ?? lines.length + 1) - 1;
      const body = lines.slice(start.line - 1, end).join('\n');
      const refs = [...body.matchAll(/(?:_unitOfWork|_unit|unit|_context|_db)\.(\w+)/g)].map(m => m[1]);
      const operations = [...body.matchAll(/\b(?:\w+\.)?((?:Create|Update|Delete|Upload|Assign|End|Remove|Add|Record|Stop|Reactivate|ManualActivate|Correct|Handle|Set|Process|Generate|Approve|Reject|Import|Export|Refresh|Mark|ReEvaluate|Calculate|Acknowledge|Request)\w*Async)\s*\(/g)].map(m => m[1]);
      methods.push({ path: item.path, ...start, end, repositories: [...new Set(refs)], calls: [...new Set(operations)], review: /RequestReviewAsync/.test(body), save: /SaveChangesAsync/.test(body), lock: /LockFileReviewAsync|LockSalaryBatchAsync/.test(body), transaction: /BeginTransactionAsync|UnitOfWorkTransaction/.test(body), softDelete: /IsDeleted\s*=\s*true/.test(body), hardDelete: /\.(?:Delete|DeleteRange)\(/.test(body) });
    });
  }
  if (item.path.startsWith('ui/src/') && /\.(?:tsx?|jsx?)$/.test(item.path)) {
    for (const match of source.matchAll(/(?:from\s*|import\s*\()(['"])([^'"\n]+)\1/g)) {
      imports.push({ path: item.path, line: source.slice(0, match.index).split('\n').length, target: match[2] });
    }
  }
}
fs.writeFileSync(path.join(out, 'methods.json'), JSON.stringify(methods, null, 2) + '\n', 'utf8');
fs.writeFileSync(path.join(out, 'frontend-imports.json'), JSON.stringify(imports, null, 2) + '\n', 'utf8');
const relevant = methods.filter(m => /Services\/(?:Pension\/(?:Beneficiaries|Entitlements|Files)|Documents|Payroll)/.test(m.path) && m.access === 'public' && /^(?:Create|Update|Delete|Upload|Assign|End|Remove|Add|Record|Stop|Deactivate|Reactivate|ManualActivate|Correct|Handle|Change|Set|Process|Generate|Approve|Reject|Review|Import|Export|Refresh|Mark|ReEvaluate|Calculate|Acknowledge|Request)/.test(m.name));
fs.writeFileSync(path.join(out, 'domain-mutations.json'), JSON.stringify(relevant, null, 2) + '\n', 'utf8');
console.log(JSON.stringify({ methods: methods.length, frontend_imports: imports.length, domain_mutation_entrypoints: relevant.length }));
